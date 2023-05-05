namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record NexusModsUserToSteamEntity : IEntity
    {
        public required int NexusModsUserId { get; init; }
        public required string SteamId { get; init; }
    }
}