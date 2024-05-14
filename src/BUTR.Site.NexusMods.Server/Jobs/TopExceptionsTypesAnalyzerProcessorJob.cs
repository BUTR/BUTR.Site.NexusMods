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
public sealed class TopExceptionsTypesAnalyzerProcessorJob : IJob
{
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public TopExceptionsTypesAnalyzerProcessorJob(ILogger<TopExceptionsTypesAnalyzerProcessorJob> logger, IServiceScopeFactory serviceScopeFactory)
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

        context.Result = "Updated Top Exception Types";
        context.SetIsSuccess(true);
    }

    private async Task HandleTenantAsync(AsyncServiceScope scope, TenantId tenant, CancellationToken ct)
    {
        var unitOfWorkFactory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();
        await using var unitOfRead = unitOfWorkFactory.CreateUnitOfRead();
        await using var unitOfWrite = unitOfWorkFactory.CreateUnitOfWrite();

        var exceptionTypes = await unitOfRead.ExceptionTypes.GetAllAsync(null, null, ct);
        var statistics = exceptionTypes.Select(x => new StatisticsTopExceptionsTypeEntity
        {
            TenantId = tenant,
            ExceptionTypeId = x.ExceptionTypeId,
            ExceptionType = unitOfWrite.UpsertEntityFactory.GetOrCreateExceptionType(x.ExceptionTypeId),
            ExceptionCount = x.ToCrashReports.Count
        }).ToList();

        unitOfWrite.StatisticsTopExceptionsTypes.Remove(x => true);
        unitOfWrite.StatisticsTopExceptionsTypes.UpsertRange(statistics);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
    }
}