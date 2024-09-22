using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsUserToIntegrationDiscordEntity : IEntity
{
    public required NexusModsUserId NexusModsUserId { get; init; }
    public required NexusModsUserEntity NexusModsUser { get; init; }
    public IntegrationDiscordTokensEntity? ToTokens { get; init; }

    public required string DiscordUserId { get; init; }

    public override int GetHashCode() => HashCode.Combine(NexusModsUserId, DiscordUserId);
}