using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsUserToIntegrationGitHubEntity : IEntity
{
    public required NexusModsUserEntity NexusModsUser { get; init; }
    public IntegrationGitHubTokensEntity? ToTokens { get; init; }

    public required string GitHubUserId { get; init; }


    public override int GetHashCode() => HashCode.Combine(NexusModsUser.NexusModsUserId, GitHubUserId);
}