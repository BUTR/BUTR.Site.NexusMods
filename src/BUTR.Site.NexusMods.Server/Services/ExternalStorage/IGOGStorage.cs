using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Shared;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public sealed record GOGOAuthTokens(string UserId, string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);

public interface IGOGStorage
{
    Task<bool> CheckOwnedGamesAsync(int nexusModsUserId, string gogUserId, GOGOAuthTokens tokens);

    Task<GOGOAuthTokens?> GetAsync(string gogUserId);
    Task<bool> UpsertAsync(int nexusModsUserId, string gogUserId, GOGOAuthTokens tokens);
    Task<bool> RemoveAsync(int nexusModsUserId, string gogUserId);
}

public sealed class DatabaseGOGStorage : IGOGStorage
{
    private Dictionary<Tenant, HashSet<uint>> TenantToGameIds { get; } = new()
    {
        { Tenant.Bannerlord, new HashSet<uint> { 1802539526, 1564781494 } }
    };

    private readonly IAppDbContextRead _dbContextRead;
    private readonly IAppDbContextWrite _dbContextWrite;
    private readonly GOGEmbedClient _gogEmbedClient;

    public DatabaseGOGStorage(IAppDbContextRead dbContextRead, IAppDbContextWrite dbContextWrite, GOGEmbedClient gogEmbedClient)
    {
        _dbContextRead = dbContextRead;
        _dbContextWrite = dbContextWrite;
        _gogEmbedClient = gogEmbedClient;
    }

    public async Task<bool> CheckOwnedGamesAsync(int nexusModsUserId, string gogUserId, GOGOAuthTokens tokens)
    {
        var entityFactory = _dbContextWrite.CreateEntityFactory();
        await using var _ = _dbContextWrite.CreateSaveScope();

        var games = await _gogEmbedClient.GetGamesAsync(tokens.AccessToken, CancellationToken.None);
        if (games is null)
            return false;

        var nexusModsUserToIntegrationGOG = entityFactory.GetOrCreateNexusModsUserGOG(nexusModsUserId, gogUserId);

        var ownedTenants = TenantToGameIds.Where(x => x.Value.Intersect(games.Owned.Where(y => y.HasValue).Select(y => y!.Value)).Any());
        var list = ownedTenants.Select(x => x.Key).Select(x => new IntegrationGOGToOwnedTenantEntity
        {
            GOGUserId = gogUserId,
            OwnedTenant = x,
        }).ToList();

        _dbContextWrite.FutureUpsert(x => x.NexusModsUserToGOG, nexusModsUserToIntegrationGOG);
        _dbContextWrite.FutureUpsert(x => x.IntegrationGOGToOwnedTenants, list);
        await _dbContextWrite.IntegrationGOGToOwnedTenants.Where(x => x.GOGUserId == gogUserId).ExecuteDeleteAsync(CancellationToken.None);
        return true;
    }

    public async Task<GOGOAuthTokens?> GetAsync(string gogUserId)
    {
        var entity = await _dbContextRead.IntegrationGOGTokens.FirstOrDefaultAsync(x => x.GOGUserId.Equals(gogUserId));
        if (entity is null) return null;
        return new(gogUserId, entity.AccessToken, entity.RefreshToken, entity.AccessTokenExpiresAt);
    }

    public async Task<bool> UpsertAsync(int nexusModsUserId, string gogUserId, GOGOAuthTokens tokens)
    {
        var entityFactory = _dbContextWrite.CreateEntityFactory();
        await using var _ = _dbContextWrite.CreateSaveScope();

        var nexusModsUserToIntegrationGOG = entityFactory.GetOrCreateNexusModsUserGOG(nexusModsUserId, gogUserId);
        var tokensGOG = entityFactory.GetOrCreateIntegrationGOGTokens(nexusModsUserId, gogUserId, tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresAt);

        _dbContextWrite.FutureUpsert(x => x.NexusModsUserToGOG, nexusModsUserToIntegrationGOG);
        _dbContextWrite.FutureUpsert(x => x.IntegrationGOGTokens, tokensGOG);
        return true;
    }

    public async Task<bool> RemoveAsync(int nexusModsUserId, string gogUserId)
    {
        await _dbContextWrite.NexusModsUserToGOG.Where(x => x.NexusModsUser.NexusModsUserId == nexusModsUserId && x.GOGUserId == gogUserId).ExecuteDeleteAsync();
        await _dbContextWrite.IntegrationGOGTokens.Where(x => x.GOGUserId == gogUserId).ExecuteDeleteAsync();

        return true;
    }
}