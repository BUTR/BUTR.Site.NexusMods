namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsUserToDiscordEntity : IEntity
{
    public required int NexusModsUserId { get; init; }
    public required string DiscordId { get; init; }
}