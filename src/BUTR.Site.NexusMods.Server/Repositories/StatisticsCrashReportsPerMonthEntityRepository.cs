using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using EFCore.BulkExtensions;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

[ScopedService<IStatisticsCrashReportsPerMonthEntityRepositoryWrite, IStatisticsCrashReportsPerMonthEntityRepositoryRead>]
internal class StatisticsCrashReportsPerMonthEntityRepository : Repository<StatisticsCrashReportsPerMonthEntity>, IStatisticsCrashReportsPerMonthEntityRepositoryWrite
{
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public StatisticsCrashReportsPerMonthEntityRepository(IAppDbContextProvider appDbContextProvider, ITenantContextAccessor tenantContextAccessor) : base(appDbContextProvider.Get())
    {
        _tenantContextAccessor = tenantContextAccessor;
    }

    protected override IQueryable<StatisticsCrashReportsPerMonthEntity> InternalQuery => base.InternalQuery;

    public async Task<IList<StatisticsCrashReportsPerDateModel>> GetAllAsync(DateOnly from, DateOnly to, NexusModsModId[]? modIds, GameVersion[]? gameVersions, ModuleId[]? moduleIds,
        ModuleVersion[]? moduleVersions, CancellationToken ct)
    {
        return await _dbContext.StatisticsCrashReportsPerMonths
            .Where(x => x.Date >= from && x.Date <= to)
            .WhereIf(modIds is { Length: > 0 }, x => modIds!.Contains(x.NexusModsModId))
            .WhereIf(gameVersions is { Length: > 0 }, x => gameVersions!.Contains(x.GameVersion))
            .WhereIf(moduleIds is { Length: > 0 }, x => moduleIds!.Contains(x.ModuleId))
            .WhereIf(moduleVersions is { Length: > 0 }, x => moduleVersions!.Contains(x.ModuleVersion))
            .OrderBy(y => y.Date).ThenBy(y => y.ModuleId).ThenBy(y => y.ModuleVersion)
            .Select(x => new StatisticsCrashReportsPerDateModel
            {
                Date = x.Date,
                GameVersion = x.GameVersion,
                NexusModsModId = x.NexusModsModId,
                ModuleId = x.ModuleId,
                ModuleVersion = x.ModuleVersion,
                Count = x.Count
            }).ToListAsync(ct);
    }

    public async Task CalculateAsync(DateOnly month, CancellationToken ct)
    {
        if (month.Day != 1)
            month = new DateOnly(month.Year, month.Month, 1);

        if (_dbContext is not AppDbContextWrite dbContextWrite)
            throw AppDbContextRead.WriteNotSupported();

        var tenant = _tenantContextAccessor.Current;

        var statistics = dbContextWrite.CrashReports.Where(x => x.CreatedAt.Month == month.Month && x.CreatedAt.Year == month.Year)
            .SelectMany(x => x.ModuleInfos.Where(y => y.IsInvolved).Select(y => new
            {
                x.GameVersion,
                y.NexusModsModId,
                y.ModuleId,
                y.Version,
            })).GroupBy(x => new
            {
                x.GameVersion,
                x.NexusModsModId,
                x.ModuleId,
                x.Version,
            }).Select(x => new StatisticsCrashReportsPerMonthEntity
            {
                TenantId = tenant,
                Date = month,
                GameVersion = x.Key.GameVersion,
                NexusModsModId = x.Key.NexusModsModId ?? NexusModsModId.None,
                ModuleId = x.Key.ModuleId,
                ModuleVersion = x.Key.Version,
                Count = x.Count()
            }).ToList();

        await dbContextWrite.StatisticsCrashReportsPerMonths.Where(x => x.Date.Month == month.Month && x.Date.Year == month.Year).ExecuteDeleteAsync(cancellationToken: ct);
        await dbContextWrite.BulkInsertOrUpdateAsync(statistics, cancellationToken: ct);
    }
}