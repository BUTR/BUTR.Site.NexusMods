using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using System;

namespace BUTR.Site.NexusMods.Server.Services;

public sealed record DiscordOAuthTokens(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);

public interface IDiscordStorage
{
    DiscordOAuthTokens? Get(string userId);
    bool Upsert(int nexusModsUserId, string discordUserId, DiscordOAuthTokens tokens);
    bool Remove(int nexusModsUserId, string discordUserId);
}

public sealed class DatabaseDiscordStorage : BaseDatabaseStorage<DiscordOAuthTokens, DiscordLinkedRoleTokensEntity, NexusModsUserToDiscordEntity>, IDiscordStorage
{
    protected override string ExternalMetadataId => ExternalStorageConstants.Discord;
    
    public DatabaseDiscordStorage(AppDbContext dbContext) : base(dbContext) { }

    protected override DiscordOAuthTokens FromExternalEntity(DiscordLinkedRoleTokensEntity externalEntity) =>
        new(externalEntity.AccessToken, externalEntity.RefreshToken, externalEntity.AccessTokenExpiresAt);

    protected override NexusModsUserToDiscordEntity? Upsert(int nexusModsUserId, string externalId, NexusModsUserToDiscordEntity? existing) => existing switch
    {
        null => new NexusModsUserToDiscordEntity
        {
            NexusModsUserId = nexusModsUserId,
            UserId = externalId,
        },
        _ => existing with
        {
            UserId = externalId,
        }
    };

    protected override DiscordLinkedRoleTokensEntity? Upsert(string externalId, DiscordOAuthTokens data, DiscordLinkedRoleTokensEntity? existing) => existing switch
    {
        null => new DiscordLinkedRoleTokensEntity
        {
            UserId = externalId,
            RefreshToken = data.RefreshToken,
            AccessToken = data.AccessToken,
            AccessTokenExpiresAt = data.ExpiresAt.ToUniversalTime()
        },
        _ => existing with
        {
            RefreshToken = data.RefreshToken,
            AccessToken = data.AccessToken,
            AccessTokenExpiresAt = data.ExpiresAt.ToUniversalTime()
        }
    };
}