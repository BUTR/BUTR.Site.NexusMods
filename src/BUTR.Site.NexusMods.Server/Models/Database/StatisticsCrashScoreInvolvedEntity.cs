namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record StatisticsCrashScoreInvolvedEntity : IEntity
{
    public required string GameVersion { get; init; }
    public required string ModId { get; init; }
    public required string ModVersion { get; init; }
    public required int InvolvedCount { get; init; }
    public required int NotInvolvedCount { get; init; }
    public required int TotalCount { get; init; }
    public required int RawValue { get; init; }
    public required double Score { get; init; }
}