namespace BUTR.Site.NexusMods.Server.Models
{
    public sealed class Filtering
    {
        public string Property { get; set; } = default!;
        public FilteringType Type { get; set; } = default!;
        public string Value { get; set; } = default!;
    }
}