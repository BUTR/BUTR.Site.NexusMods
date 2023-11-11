using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Services;

using System;
using System.Collections.Generic;
using System.Linq;

namespace BUTR.Site.NexusMods.Server.Models;

public record UserTypedMetadata
{
    public TenantId[] OwnedTenants { get; init; } = Array.Empty<TenantId>();
    public ExternalDataHolder<DiscordOAuthTokens>? Discord { get; init; }
    public ExternalDataHolder<GOGOAuthTokens>? GOG { get; init; }
    public ExternalDataHolder<Dictionary<string, string>>? Steam { get; init; }

    public UserTypedMetadata() { }

    private UserTypedMetadata(NexusModsUserEntity? userEntity)
    {
        var ownedTenants = new HashSet<TenantId>();
        if (userEntity?.ToDiscord is { ToTokens: { } tokensDiscord } discord)
        {
            Discord = new ExternalDataHolder<DiscordOAuthTokens>(discord.DiscordUserId, new(tokensDiscord.AccessToken, tokensDiscord.RefreshToken, tokensDiscord.AccessTokenExpiresAt));
        }
        if (userEntity?.ToGOG is { ToTokens: { } tokensGOG, ToOwnedTenants: { } gogOwnedTenants } gog)
        {
            GOG = new ExternalDataHolder<GOGOAuthTokens>(gog.GOGUserId, new(tokensGOG.GOGUserId, tokensGOG.AccessToken, tokensGOG.RefreshToken, tokensGOG.AccessTokenExpiresAt));
            ownedTenants.AddRange(gogOwnedTenants.Select(x => x.OwnedTenant));
        }
        if (userEntity?.ToSteam is { ToTokens: { } tokensSteam, ToOwnedTenants: { } steamOwnedTenants } steam)
        {
            Steam = new ExternalDataHolder<Dictionary<string, string>>(steam.SteamUserId, tokensSteam.Data);
            ownedTenants.AddRange(steamOwnedTenants.Select(x => x.OwnedTenant));
        }
        OwnedTenants = ownedTenants.ToArray();
    }

    public static UserTypedMetadata FromUser(NexusModsUserEntity? userEntity) => new(userEntity);
}