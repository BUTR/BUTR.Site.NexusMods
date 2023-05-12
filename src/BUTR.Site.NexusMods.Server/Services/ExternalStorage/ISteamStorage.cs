using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Services;

public interface ISteamStorage
{
    Dictionary<string, string>? Get(string userId);
    bool Upsert(int nexusModsUserId, string steamUserId, Dictionary<string, string> data);
    bool Remove(int nexusModsUserId, string steamUserId);
}

public sealed class DatabaseSteamStorage : BaseDatabaseStorage<Dictionary<string, string>, SteamLinkedRoleTokensEntity, NexusModsUserToSteamEntity>, ISteamStorage
{
    protected override string ExternalMetadataId => ExternalStorageConstants.Steam;

    public DatabaseSteamStorage(AppDbContext dbContext) : base(dbContext) { }

    protected override Dictionary<string, string> FromExternalEntity(SteamLinkedRoleTokensEntity externalEntity) => externalEntity.Data;

    protected override NexusModsUserToSteamEntity? Upsert(int nexusModsUserId, string externalId, NexusModsUserToSteamEntity? existing) => existing switch
    {
        null => new NexusModsUserToSteamEntity
        {
            NexusModsUserId = nexusModsUserId,
            UserId = externalId,
        },
        _ => existing with
        {
            UserId = externalId,
        }
    };

    protected override SteamLinkedRoleTokensEntity? Upsert(string externalId, Dictionary<string, string> data, SteamLinkedRoleTokensEntity? existing) => existing switch
    {
        null => new SteamLinkedRoleTokensEntity
        {
            UserId = externalId,
            Data = data,
        },
        _ => existing with
        {
            Data = data,
        }
    };
}