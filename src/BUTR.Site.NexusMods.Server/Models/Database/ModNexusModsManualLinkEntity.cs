namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record ModNexusModsManualLinkEntity : IEntity
    {
        public required string ModId { get; init; }

        public required int NexusModsId { get; init; }
    }
}