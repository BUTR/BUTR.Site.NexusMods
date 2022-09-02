namespace BUTR.Site.NexusMods.Server.Models
{
    public sealed record Sorting
    {
        public string Property { get; init; } = default!;
        public SortingType Type { get; init; } = default!;
    }
}