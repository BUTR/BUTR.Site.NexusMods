using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record IntegrationGitHubTokensEntity : IEntity
{
    public required NexusModsUserId NexusModsUserId { get; init; }
    public required NexusModsUserEntity NexusModsUser { get; init; }

    public required string GitHubUserId { get; init; }
    public NexusModsUserToIntegrationGitHubEntity? UserToGitHub { get; init; }

    public required string AccessToken { get; init; }

    public override int GetHashCode() => HashCode.Combine(NexusModsUserId, GitHubUserId, AccessToken);
}