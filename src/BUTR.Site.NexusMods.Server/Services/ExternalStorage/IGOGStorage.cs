using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using System;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public sealed record GOGOAuthTokens(string UserId, string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);

public interface IGOGStorage
{
    Task<GOGOAuthTokens?> GetAsync(string userId);
    Task<bool> UpsertAsync(int nexusModsUserId, string discordUserId, GOGOAuthTokens tokens);
    Task<bool> RemoveAsync(int nexusModsUserId, string discordUserId);
}

public sealed class DatabaseGOGStorage : BaseDatabaseStorage<GOGOAuthTokens, GOGLinkedRoleTokensEntity, NexusModsUserToGOGEntity>, IGOGStorage
{
    protected override string ExternalMetadataId => ExternalStorageConstants.GOG;

    public DatabaseGOGStorage(AppDbContext dbContext) : base(dbContext) { }

    protected override GOGOAuthTokens FromExternalEntity(GOGLinkedRoleTokensEntity externalEntity) =>
        new(externalEntity.UserId, externalEntity.AccessToken, externalEntity.RefreshToken, externalEntity.AccessTokenExpiresAt);

    protected override NexusModsUserToGOGEntity? Upsert(int nexusModsUserId, string externalId, NexusModsUserToGOGEntity? existing) => existing switch
    {
        null => new NexusModsUserToGOGEntity
        {
            NexusModsUserId = nexusModsUserId,
            UserId = externalId,
        },
        _ => existing with
        {
            UserId = externalId,
        }
    };

    protected override GOGLinkedRoleTokensEntity? Upsert(string externalId, GOGOAuthTokens data, GOGLinkedRoleTokensEntity? existing) => existing switch
    {
        null => new GOGLinkedRoleTokensEntity
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