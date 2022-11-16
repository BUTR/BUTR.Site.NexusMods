namespace BUTR.Site.NexusMods.Server.Models
{
    public sealed record Filtering
    {
        public required string Property { get; init; }
        public required FilteringType Type { get; init; }
        public required string Value { get; init; }
    }
}