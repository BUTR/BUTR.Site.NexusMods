﻿using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record IntegrationDiscordTokensEntity : IEntity
{
    public required NexusModsUserEntity NexusModsUser { get; init; }

    public required string DiscordUserId { get; init; }
    public NexusModsUserToIntegrationDiscordEntity? UserToDiscord { get; init; }

    /// <summary>
    /// Infinite lifetime until app is revoked
    /// </summary>
    public required string RefreshToken { get; init; }
    /// <summary>
    /// Expires when <see cref="DateTimeOffset.UtcNow"/> &gt; <see cref="AccessTokenExpiresAt"/>
    /// </summary>
    public required string AccessToken { get; init; }
    public required DateTimeOffset AccessTokenExpiresAt { get; init; }

    public override int GetHashCode() => HashCode.Combine(NexusModsUser.NexusModsUserId, DiscordUserId, RefreshToken, AccessToken, AccessTokenExpiresAt);
}