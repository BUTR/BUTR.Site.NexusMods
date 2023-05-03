namespace BUTR.Site.NexusMods.Server.Models;

public sealed record TextSearchFiltering
{
    public required TextSearchFilteringType Type { get; init; }
    public required string Value { get; init; }
}