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
public sealed class CrashReportAnalyzerProcessorJob : IJob
{
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CrashReportAnalyzerProcessorJob(ILogger<CrashReportAnalyzerProcessorJob> logger, IServiceScopeFactory serviceScopeFactory)
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

            try
            {
                await HandleTenantAsync(tenant, scope.ServiceProvider, ct);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        context.Result = "Updated Crash Report Statistics Data";
        context.SetIsSuccess(true);
    }

    private static async Task HandleTenantAsync(TenantId tenant, IServiceProvider serviceProvider, CancellationToken ct)
    {
        var dbContextRead = serviceProvider.GetRequiredService<IAppDbContextRead>();
        var dbContextWrite = serviceProvider.GetRequiredService<IAppDbContextWrite>();
        var entityFactory = dbContextWrite.GetEntityFactory();
        await using var _ = await dbContextWrite.CreateSaveScopeAsync();

        var allModVersionsQuery = dbContextRead.CrashReportModuleInfos
            .GroupBy(x => new { x.Module.ModuleId, x.Version })
            .Select(x => new { x.Key.ModuleId, x.Key.Version })
            .Distinct();

        var modCountsQuery = dbContextRead.CrashReportModuleInfos
            .Include(x => x.ToCrashReport!)
            .GroupBy(x => new { x.ToCrashReport!.GameVersion, x.Module.ModuleId, x.Version })
            .Select(x => new { x.Key.GameVersion, x.Key.ModuleId, x.Key.Version, Count = x.Count() })
            .Distinct();

        var involvedModCountsQuery = dbContextRead.CrashReportModuleInfos
            .Include(x => x.ToCrashReport!)
            .Where(x => x.IsInvolved)
            .GroupBy(x => new { x.ToCrashReport!.GameVersion, x.Module.ModuleId, x.Version })
            .Select(x => new { x.Key.GameVersion, x.Key.ModuleId, x.Key.Version, Count = x.Count() })
            .Distinct();

        var notInvolvedModCountsQuery = dbContextRead.CrashReportModuleInfos
            .Include(x => x.ToCrashReport!)
            .Where(x => !x.IsInvolved)
            .GroupBy(x => new { x.ToCrashReport!.GameVersion, x.Module.ModuleId, x.Version })
            .Select(x => new { x.Key.GameVersion, x.Key.ModuleId, x.Key.Version, Count = x.Count() })
            .Distinct();

        var query =
            from allModVersios in allModVersionsQuery
            join modCounts in modCountsQuery on new { allModVersios.ModuleId, allModVersios.Version } equals new { modCounts.ModuleId, modCounts.Version }
            join involvedModCounts in involvedModCountsQuery on new { allModVersios.ModuleId, allModVersios.Version, modCounts.GameVersion } equals new { involvedModCounts.ModuleId, involvedModCounts.Version, involvedModCounts.GameVersion }
            join notInvolvedModCounts in notInvolvedModCountsQuery on new { allModVersios.ModuleId, allModVersios.Version, modCounts.GameVersion } equals new { notInvolvedModCounts.ModuleId, notInvolvedModCounts.Version, notInvolvedModCounts.GameVersion }
            select new
            {
                GameVersion = modCounts.GameVersion,
                ModuleId = allModVersios.ModuleId,
                ModuleVersion = allModVersios.Version,
                InvolvedCount = involvedModCounts.Count,
                NotInvolvedCount = notInvolvedModCounts.Count,
                TotalCount = modCounts.Count,
                Value = involvedModCounts.Count,
                CrashScore = (double) involvedModCounts.Count / (double) modCounts.Count
            };

        var statisticsCrashScoreInvolved = await query.AsAsyncEnumerable().Select(x => new StatisticsCrashScoreInvolvedEntity
        {
            TenantId = tenant,
            StatisticsCrashScoreInvolvedId = Guid.NewGuid(),
            GameVersion = x.GameVersion,
            Module = entityFactory.GetOrCreateModule(x.ModuleId),
            ModuleVersion = x.ModuleVersion,
            InvolvedCount = x.InvolvedCount,
            NotInvolvedCount = x.NotInvolvedCount,
            TotalCount = x.TotalCount,
            RawValue = x.Value,
            Score = x.CrashScore,
        }).ToArrayAsync(ct);

        await dbContextWrite.StatisticsCrashScoreInvolveds.SynchronizeOnSaveAsync(statisticsCrashScoreInvolved);
        // Disposing the DBContext will save the data
    }
}