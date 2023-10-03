using BUTR.CrashReport.Bannerlord.Parser;
using BUTR.CrashReport.Models;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Options;
using BUTR.Site.NexusMods.Server.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Quartz;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Jobs;

[DisallowConcurrentExecution]
public sealed class CrashReportProcessorJob : IJob
{
    private class ObjectPool<T>
    {
        private readonly ConcurrentBag<T> _objects = new();
        private readonly Func<CancellationToken, Task<T>> _objectGenerator;

        public ObjectPool(Func<CancellationToken, Task<T>> objectGenerator)
        {
            _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
        }

        public async Task<T> GetAsync(CancellationToken ct) => _objects.TryTake(out var item) ? item : await _objectGenerator(ct);

        public void Return(T item) => _objects.Add(item);
    }

    private record HttpResultEntry(CrashReportFileId FileId, DateTime Date, CrashReportModel CrashReport);
    private record DatabaseResultEntry(CrashReportFileId FileId, CrashReportModel CrashReport, DateTime Date);

    private static readonly int ParallelCount = Environment.ProcessorCount / 2;

    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CrashReportProcessorJob(ILogger<CrashReportProcessorJob> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    private static async Task DownloadCrashReportsAsync(TenantId tenant, IAsyncEnumerable<CrashReportFileMetadata> requests, ILogger logger, CrashReporterClient client, ChannelWriter<HttpResultEntry> httpResultChannel, CancellationToken ct)
    {
        var options = new ParallelOptions { CancellationToken = ct, MaxDegreeOfParallelism = ParallelCount };
        await Parallel.ForEachAsync(requests, options, async (entry, ct2) =>
        {
            var (fileId, version, date) = entry;

            CrashReportModel? model;
            if (version <= 12)
            {
                var content = await client.GetCrashReportAsync(fileId, ct2);

                try
                {
                    model = CrashReportParser.ParseLegacyHtml(version, content);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to parse {FileId}", fileId);
                    throw;
                }
            }
            else
            {
                model = await client.GetCrashReportModelAsync(fileId, ct2);
            }

            if (model is null)
            {
                logger.LogError("Failed to parse {FileId}", fileId);
                return;
            }


            await httpResultChannel.WaitToWriteAsync(ct2);
            await httpResultChannel.WriteAsync(new(fileId, date, model), ct2);
        });
        httpResultChannel.Complete();
    }

    private static async Task FilterCrashReportsAsync(TenantId tenant, IServiceProvider serviceProvider, ChannelReader<HttpResultEntry> httpResultChannel, ChannelWriter<DatabaseResultEntry> databaseResultChannel, ChannelWriter<CrashReportToFileIdEntity> linkedCrashReportsChannel, ChannelWriter<CrashReportIgnoredFileEntity> ignoredCrashReportsChannel, CancellationToken ct)
    {
        var dbContextFactory = serviceProvider.GetRequiredService<IAppDbContextFactory>();
        var dbContextPool = new ObjectPool<IAppDbContextRead>(dbContextFactory.CreateReadAsync);

        var options = new ParallelOptions { CancellationToken = ct, MaxDegreeOfParallelism = ParallelCount };
        await Parallel.ForEachAsync(httpResultChannel.ReadAllAsync(ct), options, async (entry, ct2) =>
        {
            var (fileId, date, report) = entry;

            var dbContextRead = await dbContextPool.GetAsync(ct2);
            try
            {
                // Check just in case that the CrashReportToFileIdEntity is not missing for some reason
                var crashReportId = CrashReportId.From(report.Id);
                var hasCrashReport = dbContextRead.CrashReports.Any(x => x.CrashReportId == crashReportId);
                if (hasCrashReport)
                {
                    var hasCrashReportLink = dbContextRead.CrashReportToFileIds.Any(x => x.CrashReportId == crashReportId);
                    if (!hasCrashReportLink)
                    {
                        await linkedCrashReportsChannel.WriteAsync(new CrashReportToFileIdEntity
                        {
                            TenantId = tenant,
                            CrashReportId = crashReportId,
                            FileId = fileId
                        }, ct2);
                        return;
                    }
                }

                // Ignore incorrect crash reports
                var isIncorrectCrashReport = string.IsNullOrEmpty(report.GameVersion);
                var isDuplicateCrashReport = dbContextRead.CrashReportToFileIds.Any(x => x.CrashReportId == crashReportId && x.FileId != fileId);
                if (isIncorrectCrashReport || isDuplicateCrashReport)
                {
                    await ignoredCrashReportsChannel.WriteAsync(new CrashReportIgnoredFileEntity
                    {
                        TenantId = tenant,
                        Value = fileId
                    }, ct2);
                    return;
                }
            }
            finally
            {
                dbContextPool.Return(dbContextRead);
            }

            // Handle the crash report further
            await databaseResultChannel.WaitToWriteAsync(ct2);
            await databaseResultChannel.WriteAsync(new(fileId, report, date), ct2);
        });
        databaseResultChannel.Complete();
        ignoredCrashReportsChannel.Complete();
        linkedCrashReportsChannel.Complete();
    }

    private static async Task WriteIgnoredToDatabaseAsync(TenantId tenant, IServiceProvider serviceProvider, ChannelReader<CrashReportIgnoredFileEntity> ignoredCrashReportsChannel, CancellationToken ct)
    {
        var dbContextFactory = serviceProvider.GetRequiredService<IAppDbContextFactory>();
        var dbContextWrite = await dbContextFactory.CreateWriteAsync(ct);
        await foreach (var entity in ignoredCrashReportsChannel.ReadAllAsync(ct))
            dbContextWrite.CrashReportIgnoredFileIds.Add(entity);
        await dbContextWrite.SaveAsync(ct);
    }

    private static async Task WriteLinkedToDatabaseAsync(TenantId tenant, IServiceProvider serviceProvider, ChannelReader<CrashReportToFileIdEntity> linkedCrashReportsChannel, CancellationToken ct)
    {
        var dbContextFactory = serviceProvider.GetRequiredService<IAppDbContextFactory>();
        var dbContextWrite = await dbContextFactory.CreateWriteAsync(ct);
        await foreach (var entity in linkedCrashReportsChannel.ReadAllAsync(ct))
            dbContextWrite.CrashReportToFileIds.Add(entity);
        await dbContextWrite.SaveAsync(ct);
    }

    private static async Task WriteCrashReportsToDatabaseAsync(TenantId tenant, CrashReporterOptions options, IServiceProvider serviceProvider, ChannelReader<DatabaseResultEntry> databaseResultChannel, CancellationToken ct)
    {
        static string GetException(ExceptionModel? exception, bool inner = false) => exception is null ? string.Empty : $"""

{(inner ? "Inner ": string.Empty)}Exception information
Type: {exception.Type}
Message: {exception.Message}
CallStack:
{exception.CallStack}

{GetException(exception.InnerException, true)}
""";

        var dbContextFactory = serviceProvider.GetRequiredService<IAppDbContextFactory>();
        var dbContextWrite = await dbContextFactory.CreateWriteAsync(ct);
        var entityFactory = dbContextWrite.CreateEntityFactory();
        await using var _ = dbContextWrite.CreateSaveScope();

        // Filter out duplicate reports
        var uniqueIds = new HashSet<CrashReportId>();
        var ignored = new List<CrashReportIgnoredFileEntity>();
        var crashReports = new List<CrashReportEntity>();
        var crashReportMetadatas = new List<CrashReportToMetadataEntity>();
        var crashReportModules = new List<CrashReportToModuleMetadataEntity>();
        var crashReportFileIds = new List<CrashReportToFileIdEntity>();
        await foreach (var (fileId, report, date) in databaseResultChannel.ReadAllAsync(ct))
        {
            var crashReportId = CrashReportId.From(report.Id);

            if (uniqueIds.Contains(crashReportId))
            {
                ignored.Add(new CrashReportIgnoredFileEntity
                {
                    TenantId = tenant,
                    Value = fileId
                });
                continue;
            }
            uniqueIds.Add(crashReportId);

            var butrLoaderVersion = report.Metadata.AdditionalMetadata.FirstOrDefault(x => x.Key == "BUTRLoaderVersion")?.Value;
            var blseVersion = report.Metadata.AdditionalMetadata.FirstOrDefault(x => x.Key == "BLSEVersion")?.Value;
            var launcherExVersion = report.Metadata.AdditionalMetadata.FirstOrDefault(x => x.Key == "LauncherExVersion")?.Value;

            crashReports.Add(new CrashReportEntity
            {
                TenantId = tenant,
                CrashReportId = crashReportId,
                Url = CrashReportUrl.From(new Uri(new Uri(options.Endpoint), fileId.ToString())),
                Version = CrashReportVersion.From(report.Version),
                GameVersion = GameVersion.From(report.GameVersion),
                ExceptionType = entityFactory.GetOrCreateExceptionType(ExceptionTypeId.From(report.Exception.Type)),
                Exception = GetException(report.Exception),
                CreatedAt = fileId.Value.Length == 8 ? DateTime.UnixEpoch : date.ToUniversalTime(),
            });
            crashReportMetadatas.Add(new CrashReportToMetadataEntity
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
            crashReportModules.AddRange(report.Modules.Select(x => new CrashReportToModuleMetadataEntity
            {
                TenantId = tenant,
                CrashReportId = crashReportId,
                Module = entityFactory.GetOrCreateModule(ModuleId.From(x.Id)),
                Version = ModuleVersion.From(x.Version),
                NexusModsMod = NexusModsModId.TryParseUrl(x.Url, out var modId) ? entityFactory.GetOrCreateNexusModsMod(modId) : null,
                IsInvolved = report.InvolvedModules.Any(y => y.Id == x.Id),
            }));
            crashReportFileIds.Add(new CrashReportToFileIdEntity
            {
                TenantId = tenant,
                CrashReportId = crashReportId,
                FileId = fileId
            });
        }

        dbContextWrite.FutureUpsert(x => x.CrashReportIgnoredFileIds, ignored);
        dbContextWrite.FutureUpsert(x => x.CrashReports, crashReports);
        dbContextWrite.FutureUpsert(x => x.CrashReportToFileIds, crashReportFileIds);
        dbContextWrite.FutureUpsert(x => x.CrashReportModuleInfos, crashReportModules);
        dbContextWrite.FutureUpsert(x => x.CrashReportToMetadatas, crashReportMetadatas);
        // Disposing the DBContext will save the data
    }

    private static async Task HandleFileIdDatesAsync(TenantId tenant, IServiceProvider serviceProvider, ILogger logger, CrashReporterClient client, IAsyncEnumerable<CrashReportFileMetadata> requests, CancellationToken ct)
    {
        var options = serviceProvider.GetRequiredService<IOptions<CrashReporterOptions>>().Value;
        var httpResultChannel = Channel.CreateBounded<HttpResultEntry>(ParallelCount);
        var databaseResultChannel = Channel.CreateBounded<DatabaseResultEntry>(ParallelCount);
        var linkedCrashReportsChannel = Channel.CreateUnbounded<CrashReportToFileIdEntity>();
        var ignoredCrashReportsChannel = Channel.CreateUnbounded<CrashReportIgnoredFileEntity>();

        await Task.WhenAll(new Task[]
        {
            DownloadCrashReportsAsync(tenant, requests, logger, client, httpResultChannel, ct),
            FilterCrashReportsAsync(tenant, serviceProvider, httpResultChannel, databaseResultChannel, linkedCrashReportsChannel, ignoredCrashReportsChannel, ct),
            WriteIgnoredToDatabaseAsync(tenant, serviceProvider, ignoredCrashReportsChannel, ct),
            WriteLinkedToDatabaseAsync(tenant, serviceProvider, linkedCrashReportsChannel, ct),
            WriteCrashReportsToDatabaseAsync(tenant, options, serviceProvider, databaseResultChannel, ct),
        });
    }

    public async Task Execute(IJobExecutionContext context)
    {
        const int take = 2000;

        var ct = context.CancellationToken;

        var processed = 0;
        foreach (var tenant in TenantId.Values)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();

            var tenantContextAccessor = scope.ServiceProvider.GetRequiredService<ITenantContextAccessor>();
            tenantContextAccessor.Current = tenant;

            var dbContextFactory = scope.ServiceProvider.GetRequiredService<IAppDbContextFactory>();
            var dbContextRead = await dbContextFactory.CreateReadAsync(ct);

            var client = scope.ServiceProvider.GetRequiredService<CrashReporterClient>();

            var availableFileIds = await client.GetCrashReportNamesAsync(ct);
            availableFileIds.ExceptWith(await dbContextRead.CrashReportIgnoredFileIds.Select(x => x.Value).ToListAsync(ct));

            for (var toSkip = 0; toSkip < availableFileIds.Count; toSkip += take)
            {
                var toTake = availableFileIds.Count - toSkip >= take ? take : availableFileIds.Count - toSkip;

                var fileIds = availableFileIds.Skip(toSkip).Take(toTake).ToHashSet();
                fileIds.ExceptWith(await dbContextRead.CrashReportToFileIds.Where(x => fileIds.Contains(x.FileId)).Select(x => x.FileId).ToListAsync(ct));
                if (fileIds.Count == 0) continue;

                await HandleFileIdDatesAsync(tenant, scope.ServiceProvider, _logger, client, client.GetCrashReportMetadataAsync(fileIds, ct).OfType<CrashReportFileMetadata>(), ct);
                processed += fileIds.Count;
            }
        }

        context.Result = $"Processed {processed} crash reports";
        context.SetIsSuccess(true);
    }
}