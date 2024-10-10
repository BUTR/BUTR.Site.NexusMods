using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Jobs;

[DisallowConcurrentExecution]
public sealed class CrashReportProcessorJob : IJob
{
    private readonly ILogger _logger;
    private readonly ICrashReporterClient _crashReporterClient;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CrashReportProcessorJob(ILogger<CrashReportProcessorJob> logger, ICrashReporterClient crashReporterClient, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _crashReporterClient = crashReporterClient;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var ctsTimeout = new CancellationTokenSource(TimeSpan.FromMinutes(100));
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, ctsTimeout.Token);
        var ct = cts.Token;

        var processed = 0;
        foreach (var tenant in TenantId.Values)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope().WithTenant(tenant);
            var crashReportBatchedHandler = scope.ServiceProvider.GetRequiredService<ICrashReportBatchedHandler>();
            await foreach (var batch in _crashReporterClient.GetNewCrashReportMetadatasAsync(tenant, DateTime.UtcNow.AddDays(-2), ct).OfType<CrashReportFileMetadata>().ChunkAsync(100).WithCancellation(ct))
                processed += await crashReportBatchedHandler.HandleBatchAsync(batch, ct);
        }

        context.Result = $"Processed {processed} crash reports";
        context.SetIsSuccess(true);
    }
}
/*
[DisallowConcurrentExecution]
public sealed class CrashReportProcessor2Job : IJob
{
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CrashReportProcessor2Job(ILogger<CrashReportProcessor2Job> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var ctsTimeout = new CancellationTokenSource(TimeSpan.FromMinutes(10));
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, ctsTimeout.Token);
        var ct = cts.Token;

        var processed = 0;
        foreach (var tenant in TenantId.Values)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();

            var tenantContextAccessor = scope.ServiceProvider.GetRequiredService<ITenantContextAccessor>();
            tenantContextAccessor.Current = tenant;

            var client = scope.ServiceProvider.GetRequiredService<ICrashReporterClient>();

            var dbContextFactory = scope.ServiceProvider.GetRequiredService<IAppDbContextFactory>();
            var dbContextRead = await dbContextFactory.CreateReadAsync(ct);
            var dbContextWrite = await dbContextFactory.CreateWriteAsync(ct);

            var offset = 0;
            while (true)
            {
                var data = dbContextRead.CrashReports
                    .IgnoreAutoIncludes()
                    .Include(x => x.FileId)
                    .Include(x => x.ExceptionType)
                    .Where(x => x.TenantId == tenant)
                    .OrderBy(x => x.CreatedAt)
                    .Skip(offset)
                    .Take(5000)
                    .ToList();

                var entityFactory = dbContextWrite.GetEntityFactory();
                await using var __ = await dbContextWrite.CreateSaveScopeAsync();

                var toChange = new List<CrashReportEntity>();
                foreach (var crashReportEntity in data)
                {
                    if (!ExceptionTypeId.TryParseFromException(crashReportEntity.Exception, out var exception)) continue;
                    if (crashReportEntity.ExceptionType.Id == exception) continue;

                    //CrashReportModel? model;
                    //try
                    //{
                    //    var content = await client.GetCrashReportAsync(crashReportEntity.FileId!.FileId, ct);
                    //    CrashReportParser.TryParse(content, out _, out model, out _);
                    //}
                    //catch (Exception e)
                    //{
                    //    model = await client.GetCrashReportModelAsync(crashReportEntity.FileId!.FileId, ct);
                    //}
                    //var exceptionTypeEntity = entityFactory.GetOrCreateExceptionType(ExceptionTypeId.FromException(model!.Exception));
                    toChange.Add(crashReportEntity with { ExceptionType = entityFactory.GetOrCreateExceptionType(exception) });
                }

                await dbContextWrite.CrashReports.UpsertOnSaveAsync(toChange);

                offset += data.Count;
                if (data.Count == 0) break;
            }


            await using var crashReportBatchedHandler = scope.ServiceProvider.GetRequiredService<ICrashReportBatchedHandler>();

            await foreach (var batch in client.GetNewCrashReportMetadatasAsync(DateTime.UtcNow.AddDays(-2), ct).OfType<CrashReportFileMetadata>().ChunkAsync(1000).WithCancellation(ct))
            {
                processed += await crashReportBatchedHandler.HandleBatchAsync(tenant, batch, ct);
            }
        }

        context.Result = $"Processed {processed} crash reports";
        context.SetIsSuccess(true);
    }
}
*/