using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface IStatisticsCrashReportsPerMonthEntityRepositoryRead : IRepositoryRead<StatisticsCrashReportsPerMonthEntity>
{
    Task<IList<StatisticsCrashReportsPerDateModel>> GetAllAsync(DateOnly from, DateOnly to, NexusModsModId[]? modIds, GameVersion[]? gameVersions, ModuleId[]? moduleIds,
        ModuleVersion[]? moduleVersions, CancellationToken ct);
}

public interface IStatisticsCrashReportsPerMonthEntityRepositoryWrite : IRepositoryWrite<StatisticsCrashReportsPerMonthEntity>, IStatisticsCrashReportsPerMonthEntityRepositoryRead
{
    Task CalculateAsync(DateOnly month, CancellationToken ct);
}