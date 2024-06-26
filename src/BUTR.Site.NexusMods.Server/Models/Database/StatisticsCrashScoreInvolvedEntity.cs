using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record StatisticsCrashScoreInvolvedEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }

    public required Guid StatisticsCrashScoreInvolvedId { get; init; }
    public required GameVersion GameVersion { get; init; }
    public required ModuleId ModuleId { get; init; }
    public required ModuleEntity Module { get; init; }
    public required ModuleVersion ModuleVersion { get; init; }
    public required int InvolvedCount { get; init; }
    public required int NotInvolvedCount { get; init; }
    public required int TotalCount { get; init; }
    public required int RawValue { get; init; }
    public required double Score { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, StatisticsCrashScoreInvolvedId, GameVersion, ModuleId, ModuleVersion, InvolvedCount, NotInvolvedCount, HashCode.Combine(TotalCount, RawValue, Score));
}