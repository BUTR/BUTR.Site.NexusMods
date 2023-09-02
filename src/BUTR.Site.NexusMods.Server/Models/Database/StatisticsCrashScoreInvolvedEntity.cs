using BUTR.Site.NexusMods.Shared;

using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record StatisticsCrashScoreInvolvedEntity : IEntityWithTenant
{
    public required Tenant TenantId { get; init; }

    public required Guid StatisticsCrashScoreInvolvedId { get; init; }
    public required string GameVersion { get; init; }
    public required ModuleEntity Module { get; init; }
    public required string ModuleVersion { get; init; }
    public required int InvolvedCount { get; init; }
    public required int NotInvolvedCount { get; init; }
    public required int TotalCount { get; init; }
    public required int RawValue { get; init; }
    public required double Score { get; init; }

    public override int GetHashCode() => HashCode.Combine(StatisticsCrashScoreInvolvedId, GameVersion, Module.ModuleId, ModuleVersion, InvolvedCount, NotInvolvedCount, TotalCount, HashCode.Combine(RawValue, Score));
}