using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

public sealed record StatisticsCrashReportsPerDateModel
{
    public required DateOnly Date { get; init; }
    public required GameVersion GameVersion { get; init; }
    public required NexusModsModId NexusModsModId { get; init; }
    public required ModuleId ModuleId { get; init; }
    public required ModuleVersion ModuleVersion { get; init; }
    public required int Count { get; init; }
}

public interface IStatisticsCrashReportsPerDayEntityRepositoryRead : IRepositoryRead<StatisticsCrashReportsPerDayEntity>
{
    Task<IList<StatisticsCrashReportsPerDateModel>> GetAllAsync(DateOnly from, DateOnly to, NexusModsModId[]? modIds, GameVersion[]? gameVersions, ModuleId[]? moduleIds,
        ModuleVersion[]? moduleVersions, CancellationToken ct);
}

public interface IStatisticsCrashReportsPerDayEntityRepositoryWrite : IRepositoryWrite<StatisticsCrashReportsPerDayEntity>, IStatisticsCrashReportsPerDayEntityRepositoryRead
{
    Task CalculateAsync(DateOnly day, CancellationToken ct);
}