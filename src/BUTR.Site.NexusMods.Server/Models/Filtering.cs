namespace BUTR.Site.NexusMods.Server.Models
{
    public sealed record Filtering
    {
        public string Property { get; init; } = default!;
        public FilteringType Type { get; init; } = default!;
        public string Value { get; init; } = default!;
    }
}