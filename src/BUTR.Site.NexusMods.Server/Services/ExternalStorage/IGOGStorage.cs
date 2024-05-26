using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public sealed record GOGOAuthTokens(string UserId, string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);

public interface IGOGStorage
{
    Task<bool> CheckOwnedGamesAsync(NexusModsUserId nexusModsUserId, string gogUserId, GOGOAuthTokens tokens);

    Task<GOGOAuthTokens?> GetAsync(string gogUserId);
    Task<bool> UpsertAsync(NexusModsUserId nexusModsUserId, string gogUserId, GOGOAuthTokens tokens);
    Task<bool> RemoveAsync(NexusModsUserId nexusModsUserId, string gogUserId);
}

[ScopedService<IGOGStorage>]
public sealed class DatabaseGOGStorage : IGOGStorage
{
    private Dictionary<TenantId, HashSet<uint>> TenantToGameIds { get; } = new()
    {
        { TenantId.Bannerlord, [1802539526, 1564781494] },
        { TenantId.Rimworld, [1094900565] },
        { TenantId.StardewValley, [1453375253] },
    };

    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IGOGEmbedClient _gogEmbedClient;

    public DatabaseGOGStorage(IUnitOfWorkFactory unitOfWorkFactory, IGOGEmbedClient gogEmbedClient)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _gogEmbedClient = gogEmbedClient;
    }

    public async Task<bool> CheckOwnedGamesAsync(NexusModsUserId nexusModsUserId, string gogUserId, GOGOAuthTokens tokens)
    {
        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite(TenantId.None);

        var games = await _gogEmbedClient.GetGamesAsync(tokens.AccessToken, CancellationToken.None);
        if (games is null)
            return false;

        var nexusModsUserToIntegrationGOG = new NexusModsUserToIntegrationGOGEntity
        {
            NexusModsUserId = nexusModsUserId,
            NexusModsUser = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsUser(nexusModsUserId),
            GOGUserId = gogUserId,
        };

        var ownedTenants = TenantToGameIds.Where(x => x.Value.Intersect(games.Owned.Where(y => y.HasValue).Select(y => y!.Value)).Any());
        var list = ownedTenants.Select(x => x.Key).Select(x => new IntegrationGOGToOwnedTenantEntity
        {
            GOGUserId = gogUserId,
            OwnedTenant = x,
        }).ToArray();

        unitOfWrite.NexusModsUserToGOG.Upsert(nexusModsUserToIntegrationGOG);
        unitOfWrite.IntegrationGOGToOwnedTenants.UpsertRange(list);
        unitOfWrite.IntegrationGOGToOwnedTenants.Remove(x => x.GOGUserId == gogUserId);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return true;
    }

    public async Task<GOGOAuthTokens?> GetAsync(string gogUserId)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead(TenantId.None);

        var entity = await unitOfRead.IntegrationGOGTokens.FirstOrDefaultAsync(x => x.GOGUserId.Equals(gogUserId), null, CancellationToken.None);
        if (entity is null) return null;
        return new(gogUserId, entity.AccessToken, entity.RefreshToken, entity.AccessTokenExpiresAt);
    }

    public async Task<bool> UpsertAsync(NexusModsUserId nexusModsUserId, string gogUserId, GOGOAuthTokens tokens)
    {
        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite(TenantId.None);

        var nexusModsUserToIntegrationGOG = new NexusModsUserToIntegrationGOGEntity
        {
            NexusModsUserId = nexusModsUserId,
            NexusModsUser = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsUser(nexusModsUserId),
            GOGUserId = gogUserId,
        };
        var tokensGOG = new IntegrationGOGTokensEntity
        {
            GOGUserId = gogUserId,
            NexusModsUserId = nexusModsUserId,
            NexusModsUser = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsUser(nexusModsUserId),
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            AccessTokenExpiresAt = tokens.ExpiresAt,
        };

        unitOfWrite.NexusModsUserToGOG.Upsert(nexusModsUserToIntegrationGOG);
        unitOfWrite.IntegrationGOGTokens.Upsert(tokensGOG);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return true;
    }

    public async Task<bool> RemoveAsync(NexusModsUserId nexusModsUserId, string gogUserId)
    {
        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite(TenantId.None);

        unitOfWrite.NexusModsUserToGOG.Remove(x => x.NexusModsUser.NexusModsUserId == nexusModsUserId && x.GOGUserId == gogUserId);
        unitOfWrite.IntegrationGOGTokens.Remove(x => x.GOGUserId == gogUserId);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return true;
    }
}