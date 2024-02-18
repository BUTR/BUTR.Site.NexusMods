using BUTR.CrashReport.Bannerlord.Parser;
using BUTR.CrashReport.Models;
using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public interface ICrashReportBatchedHandler : IAsyncDisposable
{
    Task<int> HandleBatchAsync(IEnumerable<CrashReportFileMetadata> requests, CancellationToken ct);
}

[TransientService<ICrashReportBatchedHandler>]
public sealed class CrashReportBatchedHandler : ICrashReportBatchedHandler
{
    private record HttpResultEntry(CrashReportFileId FileId, DateTime Date, CrashReportModel? CrashReport);

    private static readonly int ParallelCount = Environment.ProcessorCount / 2;

    private static string GetException(ExceptionModel? exception, bool inner = false) => exception is null ? string.Empty : $"""

{(inner ? "Inner " : string.Empty)}Exception information
Type: {exception.Type}
Message: {exception.Message}
CallStack:
{exception.CallStack}

{GetException(exception.InnerException, true)}
""";

    private Channel<CrashReportFileMetadata> _toDownloadChannel = Channel.CreateBounded<CrashReportFileMetadata>(ParallelCount * 2);
    private Channel<HttpResultEntry> _httpResultChannel = Channel.CreateBounded<HttpResultEntry>(ParallelCount * 2);
    private Channel<CrashReportToFileIdEntity> _linkedCrashReportsChannel = Channel.CreateUnbounded<CrashReportToFileIdEntity>();
    private Channel<CrashReportIgnoredFileEntity> _ignoredCrashReportsChannel = Channel.CreateUnbounded<CrashReportIgnoredFileEntity>();

    private readonly SemaphoreSlim _lock = new(1);

    private readonly ILogger _logger;
    private readonly CrashReporterOptions _options;
    private readonly ITenantContextAccessor _tenantContextAccessor;
    private readonly IAppDbContextFactory _dbContextFactory;
    private readonly ICrashReporterClient _client;

    public CrashReportBatchedHandler(ILogger<CrashReportBatchedHandler> logger, IOptions<CrashReporterOptions> options, ITenantContextAccessor tenantContextAccessor, IAppDbContextFactory dbContextFactory, ICrashReporterClient client)
    {
        _logger = logger;
        _options = options.Value;
        _tenantContextAccessor = tenantContextAccessor;
        _dbContextFactory = dbContextFactory;
        _client = client;
    }

    public async Task<int> HandleBatchAsync(IEnumerable<CrashReportFileMetadata> requests, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);

        try
        {
            _toDownloadChannel = Channel.CreateBounded<CrashReportFileMetadata>(ParallelCount * 2);
            _httpResultChannel = Channel.CreateBounded<HttpResultEntry>(ParallelCount * 2);
            _linkedCrashReportsChannel = Channel.CreateUnbounded<CrashReportToFileIdEntity>();
            _ignoredCrashReportsChannel = Channel.CreateUnbounded<CrashReportIgnoredFileEntity>();

            var filterTask = FilterCrashReportsAsync(requests, ct);
            var downloadTask = DownloadCrashReportsAsync(ct);
            var writeTask = WriteCrashReportsToDatabaseAsync(ct);
            await Task.WhenAll(filterTask, downloadTask, writeTask);

            if (filterTask.IsFaulted || downloadTask.IsFaulted || writeTask.IsFaulted)
                throw new AggregateException(new[] { filterTask.Exception, downloadTask.Exception, writeTask.Exception }.OfType<Exception>());

            return await writeTask;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task FilterCrashReportsAsync(IEnumerable<CrashReportFileMetadata> crashReports, CancellationToken ct)
    {
        try
        {
            var tenant = _tenantContextAccessor.Current;
            var dbContextRead = await _dbContextFactory.CreateReadAsync(ct);

            var uniqueCrashReports = crashReports.DistinctBy(x => x.CrashReportId).ToArray();
            var uniqueCrashReportIds = uniqueCrashReports.Select(x => x.CrashReportId).ToArray();

            var existingCrashReportIds = dbContextRead.CrashReports.Where(x => uniqueCrashReportIds.Contains(x.CrashReportId)).Select(x => x.CrashReportId).Distinct().ToArray();
            var missingCrashReportIds = uniqueCrashReports.ExceptBy(existingCrashReportIds, x => x.CrashReportId).Select(x => x.CrashReportId).ToHashSet();

            var existingLinks = dbContextRead.CrashReportToFileIds.Where(x => existingCrashReportIds.Contains(x.CrashReportId)).Select(x => x.CrashReportId).Distinct().ToArray();
            var missingLinks = uniqueCrashReports.ExceptBy(existingLinks, x => x.CrashReportId).ToArray();
            foreach (var missingLink in missingLinks)
            {
                await _linkedCrashReportsChannel.Writer.WaitToWriteAsync(ct);
                await _linkedCrashReportsChannel.Writer.WriteAsync(new CrashReportToFileIdEntity
                {
                    TenantId = tenant,
                    CrashReportId = missingLink.CrashReportId,
                    FileId = missingLink.FileId,
                }, ct);
            }

            var duplicateLinks = dbContextRead.CrashReportToFileIds
                .Where(x => uniqueCrashReportIds.Contains(x.CrashReportId))
                .Select(x => new { x.CrashReportId, x.FileId })
                .AsEnumerable()
                .Join(uniqueCrashReports, x => new { x.CrashReportId }, x => new { x.CrashReportId }, (x, y) => new { y.CrashReportId, DbFileId = x.FileId, DlFileId = y.FileId })
                .Where(x => x.DbFileId != x.DlFileId)
                .Select(x => new { x.CrashReportId, FileId = x.DlFileId })
                .ToArray();
            foreach (var duplicateLink in duplicateLinks)
            {
                await _ignoredCrashReportsChannel.Writer.WaitToWriteAsync(ct);
                await _ignoredCrashReportsChannel.Writer.WriteAsync(new CrashReportIgnoredFileEntity
                {
                    TenantId = tenant,
                    Value = duplicateLink.FileId
                }, ct);
            }

            var crashReportsToDownload = uniqueCrashReports.Where(x => missingCrashReportIds.Contains(x.CrashReportId)).ToArray();
            foreach (var crashReport in crashReportsToDownload)
            {
                // Handle the crash report further
                await _toDownloadChannel.Writer.WaitToWriteAsync(ct);
                await _toDownloadChannel.Writer.WriteAsync(crashReport, ct);
            }
        }
        finally
        {
            _toDownloadChannel.Writer.Complete();
            _linkedCrashReportsChannel.Writer.Complete();
            _ignoredCrashReportsChannel.Writer.Complete();
        }
    }

    private async Task DownloadCrashReportsAsync(CancellationToken ct)
    {
        try
        {
            var exceptions = new ConcurrentQueue<Exception>();
            var options = new ParallelOptions { CancellationToken = ct, MaxDegreeOfParallelism = ParallelCount };
            await Parallel.ForEachAsync(_toDownloadChannel.Reader.ReadAllAsync(ct), options, async (entry, ct2) =>
            {
                try
                {
                    var (fileId, _, version, date) = entry;

                    CrashReportModel? model;
                    if (version <= 12)
                    {
                        var content = await _client.GetCrashReportAsync(fileId, ct2);

                        try
                        {
                            model = CrashReportParser.ParseLegacyHtml(version, content);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Exception while parsing Legacy HTML report {FileId}", fileId);
                            model = null;
                        }
                    }
                    else
                    {
                        model = await _client.GetCrashReportModelAsync(fileId, ct2);
                    }

                    if (model is null)
                    {
                        _logger.LogError("Failed to parse {FileId}", fileId);
                    }

                    await _httpResultChannel.Writer.WaitToWriteAsync(ct2);
                    await _httpResultChannel.Writer.WriteAsync(new(fileId, date, model), ct2);
                }
                catch (Exception e)
                {
                    exceptions.Enqueue(e);
                }
            });
            if (!exceptions.IsEmpty) throw new AggregateException(exceptions);
        }
        finally
        {
            _httpResultChannel.Writer.Complete();
        }
    }

    private async Task<int> WriteCrashReportsToDatabaseAsync(CancellationToken ct)
    {
        var tenant = _tenantContextAccessor.Current;

        var dbContextWrite = await _dbContextFactory.CreateWriteAsync(ct);
        var entityFactory = dbContextWrite.GetEntityFactory();

        var uniqueCrashReportIds = new HashSet<CrashReportId>();

        var ignoredCrashReportFileEntities = new List<CrashReportIgnoredFileEntity>();

        var failedCrashReportFileIds = new HashSet<CrashReportFileId>();

        var crashReportsBuilder = ImmutableArray.CreateBuilder<CrashReportEntity>();
        var crashReportMetadatasBuilder = ImmutableArray.CreateBuilder<CrashReportToMetadataEntity>();
        var crashReportModulesBuilder = ImmutableArray.CreateBuilder<CrashReportToModuleMetadataEntity>();
        await foreach (var (fileId, date, report) in _httpResultChannel.Reader.ReadAllAsync(ct))
        {
            if (report is null)
            {
                failedCrashReportFileIds.Add(fileId);
                continue;
            }

            var crashReportId = CrashReportId.From(report.Id);

            if (uniqueCrashReportIds.Contains(crashReportId))
            {
                ignoredCrashReportFileEntities.Add(new CrashReportIgnoredFileEntity
                {
                    TenantId = tenant,
                    Value = fileId
                });
                continue;
            }

            uniqueCrashReportIds.Add(crashReportId);

            var butrLoaderVersion = report.Metadata.AdditionalMetadata.FirstOrDefault(x => x.Key == "BUTRLoaderVersion")?.Value;
            var blseVersion = report.Metadata.AdditionalMetadata.FirstOrDefault(x => x.Key == "BLSEVersion")?.Value;
            var launcherExVersion = report.Metadata.AdditionalMetadata.FirstOrDefault(x => x.Key == "LauncherExVersion")?.Value;

            crashReportsBuilder.Add(new CrashReportEntity
            {
                TenantId = tenant,
                CrashReportId = crashReportId,
                Url = CrashReportUrl.From(new Uri(new Uri(_options.Endpoint), fileId.ToString())),
                Version = CrashReportVersion.From(report.Version),
                GameVersion = GameVersion.From(report.GameVersion),
                ExceptionType = entityFactory.GetOrCreateExceptionType(ExceptionTypeId.From(report.Exception.Type)),
                Exception = GetException(report.Exception),
                CreatedAt = fileId.Value.Length == 8 ? DateTimeOffset.UnixEpoch.ToUniversalTime() : date.ToUniversalTime(),
            });
            crashReportMetadatasBuilder.Add(new CrashReportToMetadataEntity
            {
                TenantId = tenant,
                CrashReportId = crashReportId,
                LauncherType = string.IsNullOrEmpty(report.Metadata.LauncherType) ? null : report.Metadata.LauncherType,
                LauncherVersion = string.IsNullOrEmpty(report.Metadata.LauncherVersion) ? null : report.Metadata.LauncherVersion,
                Runtime = string.IsNullOrEmpty(report.Metadata.Runtime) ? null : report.Metadata.Runtime,
                BUTRLoaderVersion = butrLoaderVersion,
                BLSEVersion = blseVersion,
                LauncherExVersion = launcherExVersion,
            });
            crashReportModulesBuilder.AddRange(report.Modules.Select(x => new CrashReportToModuleMetadataEntity
            {
                TenantId = tenant,
                CrashReportId = crashReportId,
                Module = entityFactory.GetOrCreateModule(ModuleId.From(x.Id)),
                Version = ModuleVersion.From(x.Version),
                NexusModsMod = NexusModsModId.TryParseUrl(x.Url, out var modId) ? entityFactory.GetOrCreateNexusModsMod(modId) : null,
                IsInvolved = report.InvolvedModules.Any(y => y.ModuleId == x.Id),
            }));
        }

        var linkedCrashReports = _linkedCrashReportsChannel.Reader.ReadAllAsync(ct)
            .Where(x => !failedCrashReportFileIds.Contains(x.FileId));
        var ignoredCrashReports = _ignoredCrashReportsChannel.Reader.ReadAllAsync(ct)
            .Concat(ignoredCrashReportFileEntities.ToAsyncEnumerable())
            .Concat(failedCrashReportFileIds.Select(x => new CrashReportIgnoredFileEntity
            {
                TenantId = tenant,
                Value = x,
            }).ToAsyncEnumerable());

        await using var _ = await dbContextWrite.CreateSaveScopeAsync();
        await dbContextWrite.CrashReports.UpsertOnSaveAsync(crashReportsBuilder);
        await dbContextWrite.CrashReportModuleInfos.UpsertOnSaveAsync(crashReportModulesBuilder);
        await dbContextWrite.CrashReportToMetadatas.UpsertOnSaveAsync(crashReportMetadatasBuilder);
        await dbContextWrite.CrashReportToFileIds.UpsertOnSaveAsync(linkedCrashReports);
        await dbContextWrite.CrashReportIgnoredFileIds.UpsertOnSaveAsync(ignoredCrashReports);
        // Disposing the DBContext will save the data

        return crashReportsBuilder.Count;
    }

    public ValueTask DisposeAsync()
    {
        _lock.Dispose();
        return ValueTask.CompletedTask;
    }
}