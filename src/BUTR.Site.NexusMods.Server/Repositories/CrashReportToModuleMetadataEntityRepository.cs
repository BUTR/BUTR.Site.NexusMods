using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Jobs;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using EFCore.BulkExtensions;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

[ScopedService<ICrashReportToModuleMetadataEntityRepositoryWrite, ICrashReportToModuleMetadataEntityRepositoryRead>]
internal class CrashReportToModuleMetadataEntityRepository : Repository<CrashReportToModuleMetadataEntity>, ICrashReportToModuleMetadataEntityRepositoryWrite
{
    private readonly ITenantContextAccessor _tenantContextAccessor;

    protected override IQueryable<CrashReportToModuleMetadataEntity> InternalQuery => base.InternalQuery
        .Include(x => x.ToCrashReport)
        .Include(x => x.Module)
        .Include(x => x.NexusModsMod);

    public CrashReportToModuleMetadataEntityRepository(IAppDbContextProvider appDbContextProvider, ITenantContextAccessor tenantContextAccessor) : base(appDbContextProvider.Get())
    {
        _tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<IList<StatisticsCrashReport>> GetAllStatisticsAsync(CancellationToken ct)
    {
        var allModVersionsQuery = _dbContext.CrashReportModuleInfos
            .GroupBy(x => new { x.ModuleId, x.Version })
            .Select(x => new { x.Key.ModuleId, x.Key.Version })
            .Distinct();

        var modCountsQuery = _dbContext.CrashReportModuleInfos
            .Include(x => x.ToCrashReport!)
            .GroupBy(x => new { x.ToCrashReport!.GameVersion, x.ModuleId, x.Version })
            .Select(x => new { x.Key.GameVersion, x.Key.ModuleId, x.Key.Version, Count = x.Count() })
            .Distinct();

        var involvedModCountsQuery = _dbContext.CrashReportModuleInfos
            .Include(x => x.ToCrashReport!)
            .Where(x => x.IsInvolved)
            .GroupBy(x => new { x.ToCrashReport!.GameVersion, x.ModuleId, x.Version })
            .Select(x => new { x.Key.GameVersion, x.Key.ModuleId, x.Key.Version, Count = x.Count() })
            .Distinct();

        var notInvolvedModCountsQuery = _dbContext.CrashReportModuleInfos
            .Include(x => x.ToCrashReport!)
            .Where(x => !x.IsInvolved)
            .GroupBy(x => new { x.ToCrashReport!.GameVersion, x.ModuleId, x.Version })
            .Select(x => new { x.Key.GameVersion, x.Key.ModuleId, x.Key.Version, Count = x.Count() })
            .Distinct();

        var query =
            from allModVersios in allModVersionsQuery
            join modCounts in modCountsQuery on new { allModVersios.ModuleId, allModVersios.Version } equals new { modCounts.ModuleId, modCounts.Version }
            join involvedModCounts in involvedModCountsQuery on new { allModVersios.ModuleId, allModVersios.Version, modCounts.GameVersion } equals new { involvedModCounts.ModuleId, involvedModCounts.Version, involvedModCounts.GameVersion }
            join notInvolvedModCounts in notInvolvedModCountsQuery on new { allModVersios.ModuleId, allModVersios.Version, modCounts.GameVersion } equals new { notInvolvedModCounts.ModuleId, notInvolvedModCounts.Version, notInvolvedModCounts.GameVersion }
            select new StatisticsCrashReport
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

        return await query.ToListAsync(ct);
    }

    public async Task GenerateAutoCompleteForModuleIdsAsync(CancellationToken ct)
    {
        var tenant = _tenantContextAccessor.Current;
        var key = AutocompleteProcessorProcessorJob.GenerateName<CrashReportToModuleMetadataEntity, ModuleId>(x => x.ModuleId);

        await _dbContext.Autocompletes.Where(x => x.Type == key).ExecuteDeleteAsync(ct);

        var data = await _dbContext.CrashReportModuleInfos.Select(y => y.ModuleId.Value).Distinct().Select(x => new AutocompleteEntity
        {
            AutocompleteId = default,
            TenantId = tenant,
            Type = key,
            Value = x,
        }).ToListAsync(ct);
        await _dbContext.BulkInsertOrUpdateAsync(data, cancellationToken: ct);
    }
}