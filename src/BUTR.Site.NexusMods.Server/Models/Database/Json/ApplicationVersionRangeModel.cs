namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record ApplicationVersionRangeModel
{
    public required string? Min { get; init; }
    public required string? Max { get; init; }
}