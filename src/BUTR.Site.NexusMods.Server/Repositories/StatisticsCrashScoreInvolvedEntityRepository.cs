using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

[ScopedService<IStatisticsCrashScoreInvolvedEntityRepositoryWrite, IStatisticsCrashScoreInvolvedEntityRepositoryRead>]
internal class StatisticsCrashScoreInvolvedEntityRepository : Repository<StatisticsCrashScoreInvolvedEntity>, IStatisticsCrashScoreInvolvedEntityRepositoryWrite
{
    protected override IQueryable<StatisticsCrashScoreInvolvedEntity> InternalQuery => base.InternalQuery
        .Include(x => x.Module);

    public StatisticsCrashScoreInvolvedEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }

    public async Task<IList<StatisticsInvolvedModuleScoresForGameVersionModel>> GetAllInvolvedModuleScoresForGameVersionAsync(GameVersion[]? gameVersions, ModuleId[]? moduleIds, ModuleVersion[]? moduleVersions, CancellationToken ct) => await _dbContext.StatisticsCrashScoreInvolveds
        .Include(x => x.Module)
        .WhereIf(gameVersions != null && gameVersions.Length != 0, x => gameVersions!.Contains(x.GameVersion))
        .WhereIf(moduleIds != null && moduleIds.Length != 0, x => moduleIds!.Contains(x.Module.ModuleId))
        .WhereIf(moduleVersions != null && moduleVersions.Length != 0, x => moduleVersions!.Contains(x.ModuleVersion))
        .GroupBy(x => new { x.GameVersion })
        .Select(x => new StatisticsInvolvedModuleScoresForGameVersionModel
        {
            GameVersion = x.Key.GameVersion,
            Modules = x.GroupBy(y => new { y.Module.ModuleId }).Select(y => new ModuleStorageModel
            {
                ModuleId = y.Key.ModuleId,
                Versions = y.GroupBy(z => new { z.ModuleVersion }).Select(z => new VersionStorageModel
                {
                    Version = z.Key.ModuleVersion,
                    Scores = z.Select(q => new VersionScoreModel
                    {
                        Version = z.Key.ModuleVersion,
                        Score = 1 - q.Score,
                        Value = q.RawValue,
                        CountStable = q.NotInvolvedCount,
                        CountUnstable = q.InvolvedCount,
                    }).ToArray(),
                }).ToArray(),
            }).ToArray(),
        }).ToListAsync(ct);

    public async Task<IList<StatisticsRawScoresForModuleModel>> GetAllRawScoresForAllModulesAsync(GameVersion gameVersion, ModuleId[] moduleIds, CancellationToken ct) => await _dbContext.StatisticsCrashScoreInvolveds
        .Where(x => x.GameVersion == gameVersion && moduleIds.Contains(x.Module.ModuleId))
        .GroupBy(x => x.Module.ModuleId)
        .Select(x => new StatisticsRawScoresForModuleModel
        {
            ModuleId = x.Key,
            RawScores = x.OrderBy(y => y.Score).Select(y => new RawScoreForModuleVersionModel
            {
                ModuleVersion = y.ModuleVersion,
                RawScore = y.Score,
                TotalCount = y.TotalCount,
            }).Take(10).ToArray(),
        }).ToListAsync(ct);
}