namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record ModNexusModsManualLinkTableEntry
    {
        public string ModId { get; set; } = default!;

        public int NexusModsId { get; set; } = default!;
    }
}