namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record NexusModsModEntity : IEntity
    {
        public int ModId { get; set; } = default!;

        public string Name { get; set; } = default!;

        public int[] UserIds { get; set; } = default!;
    }
}