using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsUserToIntegrationGitHubEntity : IEntity
{
    public required NexusModsUserId NexusModsUserId { get; init; }
    public required NexusModsUserEntity NexusModsUser { get; init; }
    public IntegrationGitHubTokensEntity? ToTokens { get; init; }

    public required string GitHubUserId { get; init; }


    public override int GetHashCode() => HashCode.Combine(NexusModsUserId, GitHubUserId);
}