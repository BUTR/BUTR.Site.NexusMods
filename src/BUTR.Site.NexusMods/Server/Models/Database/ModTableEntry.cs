namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public record ModTableEntry
    {
        public string Name { get; set; } = default!;

        public string GameDomain { get; set; } = default!;

        public int ModId { get; set; } = default!;

        public int[] UserIds { get; set; } = default!;
    }
}