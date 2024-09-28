using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public abstract record StatisticsCrashReportsPerDateEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }

    public required DateOnly Date { get; init; }
    public required GameVersion GameVersion { get; init; }
    public required NexusModsModId NexusModsModId { get; init; }
    public required ModuleId ModuleId { get; init; }
    public required ModuleVersion ModuleVersion { get; init; }
    public required int Count { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, Date, GameVersion, NexusModsModId, ModuleId, ModuleVersion, Count);
}

public sealed record StatisticsCrashReportsPerDayEntity : StatisticsCrashReportsPerDateEntity;

public sealed record StatisticsCrashReportsPerMonthEntity : StatisticsCrashReportsPerDateEntity;