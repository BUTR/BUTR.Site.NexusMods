using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public interface ISteamStorage
{
    Task<bool> CheckOwnedGamesAsync(NexusModsUserId nexusModsUserId, string steamUserId);

    Task<Dictionary<string, string>?> GetAsync(string steamUserId);
    Task<bool> UpsertAsync(NexusModsUserId nexusModsUserId, string steamUserId, Dictionary<string, string> data);
    Task<bool> RemoveAsync(NexusModsUserId nexusModsUserId, string steamUserId);
}

[ScopedService<ISteamStorage>]
public sealed class DatabaseSteamStorage : ISteamStorage
{
    private Dictionary<TenantId, HashSet<uint>> TenantToGameIds { get; } = new()
    {
        { TenantId.Bannerlord, [261550]},
        { TenantId.Rimworld, [294100]},
        { TenantId.StardewValley, [413150]},
    };

    private readonly IAppDbContextRead _dbContextRead;
    private readonly IAppDbContextWrite _dbContextWrite;
    private readonly ISteamAPIClient _steamAPIClient;

    public DatabaseSteamStorage(IAppDbContextRead dbContextRead, IAppDbContextWrite dbContextWrite, ISteamAPIClient steamAPIClient)
    {
        _dbContextRead = dbContextRead;
        _dbContextWrite = dbContextWrite;
        _steamAPIClient = steamAPIClient;
    }

    public async Task<bool> CheckOwnedGamesAsync(NexusModsUserId nexusModsUserId, string steamUserId)
    {
        var entityFactory = _dbContextWrite.GetEntityFactory();
        await using var _ = await _dbContextWrite.CreateSaveScopeAsync();

        var nexusModsUserToIntegrationSteam = entityFactory.GetOrCreateNexusModsUserSteam(nexusModsUserId, steamUserId);

        var list = ImmutableArray.CreateBuilder<IntegrationSteamToOwnedTenantEntity>();
        foreach (var (tenant, gameIds) in TenantToGameIds)
        {
            var ownsTenant = false;
            foreach (var gameId in gameIds)
            {
                ownsTenant = await _steamAPIClient.IsOwningGameAsync(steamUserId, gameId, CancellationToken.None);
                if (ownsTenant) break;
            }

            if (ownsTenant)
            {
                list.Add(new IntegrationSteamToOwnedTenantEntity()
                {
                    SteamUserId = steamUserId,
                    OwnedTenant = tenant,
                });
            }
        }

        await _dbContextWrite.NexusModsUserToSteam.UpsertOnSaveAsync(nexusModsUserToIntegrationSteam);
        await _dbContextWrite.IntegrationSteamToOwnedTenants.UpsertOnSaveAsync(list.ToArray());
        await _dbContextWrite.IntegrationSteamToOwnedTenants.Where(x => x.SteamUserId == steamUserId).ExecuteDeleteAsync(CancellationToken.None);
        return true;
    }

    public async Task<Dictionary<string, string>?> GetAsync(string steamUserId)
    {
        var entity = await _dbContextRead.IntegrationSteamTokens.FirstOrDefaultAsync(x => x.SteamUserId.Equals(steamUserId));
        if (entity is null) return null;
        return entity.Data;
    }

    public async Task<bool> UpsertAsync(NexusModsUserId nexusModsUserId, string steamUserId, Dictionary<string, string> data)
    {
        var entityFactory = _dbContextWrite.GetEntityFactory();
        await using var _ = await _dbContextWrite.CreateSaveScopeAsync();

        var nexusModsUserToIntegrationSteam = entityFactory.GetOrCreateNexusModsUserSteam(nexusModsUserId, steamUserId);
        var tokensSteam = entityFactory.GetOrCreateIntegrationSteamTokens(nexusModsUserId, steamUserId, data);

        await _dbContextWrite.NexusModsUserToSteam.UpsertOnSaveAsync(nexusModsUserToIntegrationSteam);
        await _dbContextWrite.IntegrationSteamTokens.UpsertOnSaveAsync(tokensSteam);
        return true;
    }

    public async Task<bool> RemoveAsync(NexusModsUserId nexusModsUserId, string steamUserId)
    {
        await _dbContextWrite.NexusModsUserToSteam.Where(x => x.NexusModsUser.NexusModsUserId == nexusModsUserId && x.SteamUserId == steamUserId).ExecuteDeleteAsync();
        await _dbContextWrite.IntegrationSteamTokens.Where(x => x.SteamUserId == steamUserId).ExecuteDeleteAsync();

        return true;
    }
}