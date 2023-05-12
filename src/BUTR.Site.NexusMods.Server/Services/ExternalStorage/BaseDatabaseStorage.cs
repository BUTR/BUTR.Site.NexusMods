using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BUTR.Site.NexusMods.Server.Services;

public interface IExternalEntity : IEntity
{
    string UserId { get; }
}

public interface INexusModsToExternalEntity : IEntity
{
    int NexusModsUserId { get; }
    string UserId { get; }
}

public record ExternalDataHolder<TData>(string ExternalId, TData Data) where TData : class;

public abstract class BaseDatabaseStorage<TData, TExternalEntity, TNexusModsToExternalEntity>
    where TData : class
    where TExternalEntity : class, IExternalEntity
    where TNexusModsToExternalEntity : class, INexusModsToExternalEntity
{
    protected abstract string ExternalMetadataId { get; }
    
    private readonly AppDbContext _dbContext;

    protected BaseDatabaseStorage(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    protected abstract TData FromExternalEntity(TExternalEntity externalEntity);
    
    protected abstract TNexusModsToExternalEntity? Upsert(int nexusModsUserId, string externalId, TNexusModsToExternalEntity? existing);
    protected abstract TExternalEntity? Upsert(string externalId, TData data, TExternalEntity? existing);
    private TNexusModsToExternalEntity? Remove(TNexusModsToExternalEntity? existing) => existing switch
    {
        _ => null
    };
    private TExternalEntity? Remove(TExternalEntity? existing) => existing switch
    {
        _ => null
    };
    
    
    public TData? Get(string userId)
    {
        var entity = _dbContext.FirstOrDefault<TExternalEntity>(x => x.UserId == userId);
        return entity is not null ? FromExternalEntity(entity) : null;
    }

    public bool Upsert(int nexusModsUserId, string externalUserId, TData data)
    {
        if (!_dbContext.AddUpdateRemoveAndSave<TNexusModsToExternalEntity>(x => x.NexusModsUserId == nexusModsUserId, entity => Upsert(nexusModsUserId, externalUserId, entity)))
            return false;

        if (!_dbContext.AddUpdateRemoveAndSave<TExternalEntity>(x => x.UserId == externalUserId, entity => Upsert(externalUserId, data, entity)))
            return false;

        NexusModsUserMetadataEntity? ApplyChanges(NexusModsUserMetadataEntity? existing) => existing switch
        {
            null => new NexusModsUserMetadataEntity
            {
                NexusModsUserId = nexusModsUserId,
                Metadata = new Dictionary<string, string>
                {
                    {ExternalMetadataId, JsonSerializer.Serialize(new ExternalDataHolder<TData>(externalUserId, data))}
                }
            },
            //_ when existing.Metadata.TryGetValue("DiscordTokens", out var json) => JsonSerializer.Deserialize<DiscordUserTokens>(json) is { } eTokens && eTokens.RefreshToken != tokens.RefreshToken
            //    ? existing
            //    : existing,
            //_ when !existing.Metadata.ContainsKey("DiscordTokens") => existing with
            //{
            //    Metadata = existing.Metadata.AddAndReturn("DiscordTokens", JsonSerializer.Serialize(new DiscordUserTokens(discordUserId, tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresAt)))
            //},
            _ => existing with
            {
                Metadata = existing.Metadata.SetAndReturn(ExternalMetadataId, JsonSerializer.Serialize(new ExternalDataHolder<TData>(externalUserId, data)))
            },
        };
        if (!_dbContext.AddUpdateRemoveAndSave<NexusModsUserMetadataEntity>(x => x.NexusModsUserId == nexusModsUserId, ApplyChanges))
            return false;

        return true;
    }

    public bool Remove(int nexusModsUserId, string externalUserId)
    {
        if (!_dbContext.AddUpdateRemoveAndSave<TNexusModsToExternalEntity>(x => x.NexusModsUserId == nexusModsUserId, Remove))
            return false;

        if (!_dbContext.AddUpdateRemoveAndSave<TExternalEntity>(x => x.UserId == externalUserId, Remove))
            return false;

        NexusModsUserMetadataEntity? ApplyChanges(NexusModsUserMetadataEntity? existing) => existing switch
        {
            null => null,
            _ when existing.Metadata.ContainsKey(ExternalMetadataId) => existing with
            {
                Metadata = existing.Metadata.RemoveAndReturn<Dictionary<string, string>, string, string>(ExternalMetadataId)
            },
            _ => existing
        };
        if (!_dbContext.AddUpdateRemoveAndSave<NexusModsUserMetadataEntity>(x => x.NexusModsUserId == nexusModsUserId, ApplyChanges))
            return false;

        return true;
    }
}