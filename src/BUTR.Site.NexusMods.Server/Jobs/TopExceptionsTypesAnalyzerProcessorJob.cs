using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
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
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var ctsTimeout = new CancellationTokenSource(TimeSpan.FromMinutes(10));
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, ctsTimeout.Token);
        var ct = cts.Token;

        foreach (var tenant in TenantId.Values)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();

            var tenantContextAccessor = scope.ServiceProvider.GetRequiredService<ITenantContextAccessor>();
            tenantContextAccessor.Current = tenant;

            await HandleTenantAsync(tenant, scope.ServiceProvider, ct);
        }

        context.Result = "Updated Top Exception Types";
        context.SetIsSuccess(true);
    }

    private static async Task HandleTenantAsync(TenantId tenant, IServiceProvider serviceProvider, CancellationToken ct)
    {
        var dbContextRead = serviceProvider.GetRequiredService<IAppDbContextRead>();
        var dbContextWrite = serviceProvider.GetRequiredService<IAppDbContextWrite>();
        var entityFactory = dbContextWrite.GetEntityFactory();
        await using var _ = await dbContextWrite.CreateSaveScopeAsync();

        var statisticsQuery = await dbContextRead.ExceptionTypes.Include(x => x.ToCrashReports).AsSplitQuery().Select(x => new StatisticsTopExceptionsTypeEntity
        {
            TenantId = tenant,
            ExceptionType = entityFactory.GetOrCreateExceptionType(x.ExceptionTypeId),
            ExceptionCount = x.ToCrashReports.Count
        }).ToArrayAsync(ct);

        await dbContextWrite.StatisticsTopExceptionsTypes.SynchronizeOnSaveAsync(statisticsQuery);
        // Disposing the DBContext will save the data
    }
}