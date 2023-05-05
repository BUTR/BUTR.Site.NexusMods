using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BUTR.Site.NexusMods.Server.Services;

public interface ISteamStorage
{
    Dictionary<string, string>? Get(string userId);
    bool Upsert(int nexusModsUserId, string steamUserId, Dictionary<string, string> data);
    bool Remove(int nexusModsUserId, string steamUserId);
}

public sealed class DatabaseSteamStorage : ISteamStorage
{
    private readonly AppDbContext _dbContext;

    public DatabaseSteamStorage(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Dictionary<string, string>? Get(string userId)
    {
        var entity = _dbContext.FirstOrDefault<SteamLinkedRoleTokensEntity>(x => x.UserId == userId);
        return entity is not null ? entity.Data : null;
    }

    public bool Upsert(int nexusModsUserId, string steamUserId, Dictionary<string, string> data)
    {
        NexusModsUserToSteamEntity? ApplyChanges1(NexusModsUserToSteamEntity? existing) => existing switch
        {
            null => new NexusModsUserToSteamEntity
            {
                NexusModsUserId = nexusModsUserId,
                SteamId = steamUserId,
            },
            _ => existing with
            {
                SteamId = steamUserId,
            }
        };
        if (!_dbContext.AddUpdateRemoveAndSave<NexusModsUserToSteamEntity>(x => x.NexusModsUserId == nexusModsUserId, ApplyChanges1))
            return false;

        SteamLinkedRoleTokensEntity? ApplyChanges2(SteamLinkedRoleTokensEntity? existing) => existing switch
        {
            null => new SteamLinkedRoleTokensEntity
            {
                UserId = steamUserId,
                Data = data,
            },
            _ => existing with
            {
                Data = data,
            }
        };
        if (!_dbContext.AddUpdateRemoveAndSave<SteamLinkedRoleTokensEntity>(x => x.UserId == steamUserId, ApplyChanges2))
            return false;

        NexusModsUserMetadataEntity? ApplyChanges3(NexusModsUserMetadataEntity? existing) => existing switch
        {
            null => new NexusModsUserMetadataEntity
            {
                NexusModsUserId = nexusModsUserId,
                Metadata = new Dictionary<string, string>
                {
                    {"SteamTokens", JsonSerializer.Serialize(new SteamUserTokens(steamUserId, data))}
                }
            },
            //_ when existing.Metadata.TryGetValue("SteamTokens", out var json) => JsonSerializer.Deserialize<SteamUserTokens>(json) is { } eTokens && eTokens.RefreshToken != tokens.RefreshToken
            //    ? existing
            //    : existing,
            //_ when !existing.Metadata.ContainsKey("SteamTokens") => existing with
            //{
            //    Metadata = existing.Metadata.AddAndReturn("SteamTokens", JsonSerializer.Serialize(new SteamUserTokens(steamUserId, data)))
            //},
            _ => existing with
            {
                Metadata = existing.Metadata.SetAndReturn("SteamTokens", JsonSerializer.Serialize(new SteamUserTokens(steamUserId, data)))
            },
        };
        if (!_dbContext.AddUpdateRemoveAndSave<NexusModsUserMetadataEntity>(x => x.NexusModsUserId == nexusModsUserId, ApplyChanges3))
            return false;

        return true;
    }

    public bool Remove(int nexusModsUserId, string steamUserId)
    {
        NexusModsUserToSteamEntity? ApplyChanges1(NexusModsUserToSteamEntity? existing) => existing switch
        {
            _ => null
        };
        if (!_dbContext.AddUpdateRemoveAndSave<NexusModsUserToSteamEntity>(x => x.NexusModsUserId == nexusModsUserId, ApplyChanges1))
            return false;

        SteamLinkedRoleTokensEntity? ApplyChanges(SteamLinkedRoleTokensEntity? existing) => existing switch
        {
            _ => null
        };
        if (!_dbContext.AddUpdateRemoveAndSave<SteamLinkedRoleTokensEntity>(x => x.UserId == steamUserId, ApplyChanges))
            return false;

        NexusModsUserMetadataEntity? ApplyChanges2(NexusModsUserMetadataEntity? existing) => existing switch
        {
            null => null,
            _ when existing.Metadata.ContainsKey("SteamTokens") => existing with
            {
                Metadata = existing.Metadata.RemoveAndReturn<Dictionary<string, string>, string, string>("SteamTokens")
            },
            _ => existing
        };
        if (!_dbContext.AddUpdateRemoveAndSave<NexusModsUserMetadataEntity>(x => x.NexusModsUserId == nexusModsUserId, ApplyChanges2))
            return false;

        return true;
    }
}