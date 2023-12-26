using System;
using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record IntegrationGitHubTokensEntity : IEntity
{
    public required NexusModsUserEntity NexusModsUser { get; init; }

    public required string GitHubUserId { get; init; }
    public NexusModsUserToIntegrationGitHubEntity? UserToGitHub { get; init; }

    public required string AccessToken { get; init; }

    public override int GetHashCode() => HashCode.Combine(NexusModsUser.NexusModsUserId, GitHubUserId, AccessToken);
}