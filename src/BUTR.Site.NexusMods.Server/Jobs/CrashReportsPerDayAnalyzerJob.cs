using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Repositories;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Jobs;

[DisallowConcurrentExecution]
public sealed class CrashReportsPerDayAnalyzerJob : IJob
{
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CrashReportsPerDayAnalyzerJob(ILogger<CrashReportsPerDayAnalyzerJob> logger, IServiceScopeFactory serviceScopeFactory)
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

        context.Result = "Updated Crash Reports Statistics For Previous Day";
        context.SetIsSuccess(true);
    }

    private async Task HandleTenantAsync(AsyncServiceScope scope, TenantId tenant, CancellationToken ct)
    {
        var unitOfWorkFactory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();
        await using var unitOfWrite = unitOfWorkFactory.CreateUnitOfWrite();

        await unitOfWrite.StatisticsCrashReportsPerDay.CalculateAsync(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), ct);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
    }
}