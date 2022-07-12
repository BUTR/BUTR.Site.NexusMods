namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record ModNexusModsManualLinkEntity : IEntity
    {
        public string ModId { get; set; } = default!;

        public int NexusModsId { get; set; } = default!;
    }
}