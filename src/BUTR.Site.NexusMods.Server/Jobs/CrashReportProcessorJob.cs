using BUTR.Site.NexusMods.Server.Contexts;
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
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CrashReportProcessorJob(ILogger<CrashReportProcessorJob> logger, IServiceScopeFactory serviceScopeFactory)
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
            await using var crashReportBatchedHandler = scope.ServiceProvider.GetRequiredService<ICrashReportBatchedHandler>();

            await foreach (var batch in client.GetNewCrashReportMetadatasAsync(DateTime.UtcNow.AddDays(-2), ct).OfType<CrashReportFileMetadata>().ChunkAsync(1000).WithCancellation(ct))
            {
                processed += await crashReportBatchedHandler.HandleBatchAsync(batch, ct);
            }
        }

        context.Result = $"Processed {processed} crash reports";
        context.SetIsSuccess(true);
    }
}