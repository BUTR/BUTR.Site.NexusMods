using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Repositories;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Jobs;

[DisallowConcurrentExecution]
public sealed class CrashReportAnalyzerProcessorJob : IJob
{
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CrashReportAnalyzerProcessorJob(ILogger<CrashReportAnalyzerProcessorJob> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var ctsTimeout = new CancellationTokenSource(TimeSpan.FromMinutes(10));
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, ctsTimeout.Token);
        var ct = cts.Token;

        foreach (var tenant in TenantId.Values)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope().WithTenant(tenant);
            await HandleTenantAsync(scope, tenant, ct);
        }

        context.Result = "Updated Crash Report Statistics Data";
        context.SetIsSuccess(true);
    }

    private async Task HandleTenantAsync(AsyncServiceScope scope, TenantId tenant, CancellationToken ct)
    {
        var unitOfWorkFactory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();
        await using var unitOfRead = unitOfWorkFactory.CreateUnitOfRead();
        await using var unitOfWrite = unitOfWorkFactory.CreateUnitOfWrite();

        var statisticsData = await unitOfRead.CrashReportModuleInfos.GetAllStatisticsAsync(ct);

        unitOfWrite.StatisticsCrashScoreInvolveds.Remove(x => true);
        unitOfWrite.StatisticsCrashScoreInvolveds.UpsertRange(statisticsData.Select(x => new StatisticsCrashScoreInvolvedEntity
        {
            TenantId = tenant,
            StatisticsCrashScoreInvolvedId = Guid.NewGuid(),
            GameVersion = x.GameVersion,
            ModuleId = x.ModuleId,
            Module = unitOfWrite.UpsertEntityFactory.GetOrCreateModule(x.ModuleId),
            ModuleVersion = x.ModuleVersion,
            InvolvedCount = x.InvolvedCount,
            NotInvolvedCount = x.NotInvolvedCount,
            TotalCount = x.TotalCount,
            RawValue = x.Value,
            Score = x.CrashScore,
        }).ToList());

        await unitOfWrite.SaveChangesAsync(ct);
    }
}