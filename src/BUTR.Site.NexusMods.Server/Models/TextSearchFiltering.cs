namespace BUTR.Site.NexusMods.Server.Models
{
    public sealed record TextSearchFiltering
    {
        public TextSearchFilteringType Type { get; init; } = default!;
        public string Value { get; init; } = default!;
    }
}