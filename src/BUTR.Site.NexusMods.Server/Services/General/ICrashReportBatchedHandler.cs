using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Options;
using BUTR.Site.NexusMods.Server.Repositories;

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
using BUTR.Site.NexusMods.Server.CrashReport.v13;
using BUTR.Site.NexusMods.Server.CrashReport.v14;

namespace BUTR.Site.NexusMods.Server.Services;

public interface ICrashReportBatchedHandler : IAsyncDisposable
{
    Task<int> HandleBatchAsync(IEnumerable<CrashReportFileMetadata> requests, CancellationToken ct);
}

[ScopedService<ICrashReportBatchedHandler>]
public sealed class CrashReportBatchedHandler : ICrashReportBatchedHandler
{
    private record HttpResultEntry(CrashReportFileId FileId, DateTime Date, byte Version, string? CrashReportContent);

    private static readonly int ParallelCount = Environment.ProcessorCount / 2;

    private Channel<CrashReportFileMetadata> _toDownloadChannel = Channel.CreateBounded<CrashReportFileMetadata>(ParallelCount * 2);
    private Channel<HttpResultEntry> _httpResultChannel = Channel.CreateBounded<HttpResultEntry>(ParallelCount * 2);
    private Channel<CrashReportToFileIdEntity> _linkedCrashReportsChannel = Channel.CreateUnbounded<CrashReportToFileIdEntity>();
    private Channel<CrashReportIgnoredFileEntity> _ignoredCrashReportsChannel = Channel.CreateUnbounded<CrashReportIgnoredFileEntity>();

    private readonly SemaphoreSlim _lock = new(1);

    private readonly ILogger _logger;
    private readonly CrashReporterOptions _options;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly ICrashReporterClient _client;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public CrashReportBatchedHandler(ILogger<CrashReportBatchedHandler> logger, IOptions<CrashReporterOptions> options, IUnitOfWorkFactory unitOfWorkFactory, ICrashReporterClient client, ITenantContextAccessor tenantContextAccessor)
    {
        _logger = logger;
        _options = options.Value;
        _unitOfWorkFactory = unitOfWorkFactory;
        _client = client;
        _tenantContextAccessor = tenantContextAccessor;
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
        var tenant = _tenantContextAccessor.Current;

        try
        {
            var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

            var uniqueCrashReports = crashReports.DistinctBy(x => x.CrashReportId).ToArray();
            var uniqueCrashReportIds = uniqueCrashReports.Select(x => x.CrashReportId).ToArray();

            var existingCrashReportIds = await unitOfRead.CrashReports.GetAllAsync(x => uniqueCrashReportIds.Contains(x.CrashReportId), null, x => x.CrashReportId, ct);
            var missingCrashReportIds = uniqueCrashReports.ExceptBy(existingCrashReportIds, x => x.CrashReportId).Select(x => x.CrashReportId).ToHashSet();

            var existingLinks = await unitOfRead.CrashReportToFileIds.GetAllAsync(x => existingCrashReportIds.Contains(x.CrashReportId), null, x => x.CrashReportId, ct);
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

            var uniqueCrashReportsTuple = await unitOfRead.CrashReportToFileIds
                .GetAllAsync(x => uniqueCrashReportIds.Contains(x.CrashReportId), null, x => new { x.CrashReportId, x.FileId }, ct);

            var duplicateLinks = uniqueCrashReportsTuple
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
                    CrashReportFileId = duplicateLink.FileId
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

                    string? content;
                    try
                    {
                        if (version <= 12)
                        {
                            content = await _client.GetCrashReportAsync(fileId, ct2);
                        }
                        else
                        {
                            content = await _client.GetCrashReportJsonAsync(fileId, ct2);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to download {FileId}", fileId);
                        content = null;
                    }

                    await _httpResultChannel.Writer.WaitToWriteAsync(ct2);
                    await _httpResultChannel.Writer.WriteAsync(new(fileId, date, version, content), ct2);
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

        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite();

        var uniqueCrashReportIds = new HashSet<CrashReportId>();

        var ignoredCrashReportFileEntities = new List<CrashReportIgnoredFileEntity>();

        var failedCrashReportFileIds = new HashSet<CrashReportFileId>();

        var crashReportsBuilder = ImmutableArray.CreateBuilder<CrashReportEntity>();
        var crashReportMetadatasBuilder = ImmutableArray.CreateBuilder<CrashReportToMetadataEntity>();
        var crashReportModulesBuilder = ImmutableArray.CreateBuilder<CrashReportToModuleMetadataEntity>();
        await foreach (var (fileId, date, version, content) in _httpResultChannel.Reader.ReadAllAsync(ct))
        {
            if (content is null)
            {
                failedCrashReportFileIds.Add(fileId);
                continue;
            }

            CrashReportEntity? crashReportEntity = null;
            CrashReportToMetadataEntity? crashReportToMetadataEntity = null;
            IList<CrashReportToModuleMetadataEntity>? crashReportToModuleMetadataEntities = null;

            var url = CrashReportUrl.From(new Uri(new Uri(_options.Endpoint), fileId.ToString()));
            
            if (version <= 12)
            {
                var result = CrashReportV14.TryFromHtml(
                    unitOfWrite,
                    tenant,
                    fileId,
                    url,
                    date,
                    version,
                    content,
                    out crashReportEntity,
                    out crashReportToMetadataEntity,
                    out crashReportToModuleMetadataEntities);
                if (!result)
                {
                    failedCrashReportFileIds.Add(fileId);
                    continue;
                }
            }
            if (version == 13)
            {
                var result = CrashReportV13.TryFromJson(
                    unitOfWrite,
                    tenant,
                    fileId,
                    url,
                    date,
                    version,
                    content,
                    out crashReportEntity,
                    out crashReportToMetadataEntity,
                    out crashReportToModuleMetadataEntities);
                if (!result)
                {
                    failedCrashReportFileIds.Add(fileId);
                    continue;
                }
            }
            if (version == 14)
            {
                var result = CrashReportV14.TryFromJson(
                    unitOfWrite,
                    tenant,
                    fileId,
                    url,
                    date,
                    version,
                    content,
                    out crashReportEntity,
                    out crashReportToMetadataEntity,
                    out crashReportToModuleMetadataEntities);
                if (!result)
                {
                    failedCrashReportFileIds.Add(fileId);
                    continue;
                }
            }
            
            if (crashReportEntity is null || crashReportToMetadataEntity is null || crashReportToModuleMetadataEntities is null)
            {
                failedCrashReportFileIds.Add(fileId);
                continue;
            }

            if (!uniqueCrashReportIds.Add(crashReportEntity.CrashReportId))
            {
                ignoredCrashReportFileEntities.Add(new CrashReportIgnoredFileEntity
                {
                    TenantId = tenant,
                    CrashReportFileId = fileId
                });
                continue;
            }

            crashReportsBuilder.Add(crashReportEntity);
            crashReportMetadatasBuilder.Add(crashReportToMetadataEntity);
            crashReportModulesBuilder.AddRange(crashReportToModuleMetadataEntities);
        }

        var linkedCrashReports = _linkedCrashReportsChannel.Reader.ReadAllAsync(ct)
            .Where(x => !failedCrashReportFileIds.Contains(x.FileId));
        var ignoredCrashReports = _ignoredCrashReportsChannel.Reader.ReadAllAsync(ct)
            .Concat(ignoredCrashReportFileEntities.ToAsyncEnumerable())
            .Concat(failedCrashReportFileIds.Select(x => new CrashReportIgnoredFileEntity
            {
                TenantId = tenant,
                CrashReportFileId = x,
            }).ToAsyncEnumerable());

        unitOfWrite.CrashReports.UpsertRange(crashReportsBuilder);
        unitOfWrite.CrashReportModuleInfos.UpsertRange(crashReportModulesBuilder);
        unitOfWrite.CrashReportToMetadatas.UpsertRange(crashReportMetadatasBuilder);
        unitOfWrite.CrashReportToFileIds.UpsertRange(await linkedCrashReports.ToArrayAsync(ct));
        unitOfWrite.CrashReportIgnoredFileIds.UpsertRange(await ignoredCrashReports.ToArrayAsync(ct));

        await unitOfWrite.SaveChangesAsync(ct);
        return crashReportsBuilder.Count;
    }

    public ValueTask DisposeAsync()
    {
        _lock.Dispose();
        return ValueTask.CompletedTask;
    }
}