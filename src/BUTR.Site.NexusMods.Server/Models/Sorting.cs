namespace BUTR.Site.NexusMods.Server.Models;

public sealed record Sorting
{
    public required string Property { get; init; }
    public required SortingType Type { get; init; }
}