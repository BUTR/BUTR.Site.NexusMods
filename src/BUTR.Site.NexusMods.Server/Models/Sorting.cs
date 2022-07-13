namespace BUTR.Site.NexusMods.Server.Models
{
    public sealed class Sorting
    {
        public string Property { get; set; } = default!;
        public SortingType Type { get; set; } = default!;
    }
}