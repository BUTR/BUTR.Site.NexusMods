namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record StatisticsTopExceptionsTypeResult
{
    public required string Type { get; init; }
    public required int Count { get; init; }
}