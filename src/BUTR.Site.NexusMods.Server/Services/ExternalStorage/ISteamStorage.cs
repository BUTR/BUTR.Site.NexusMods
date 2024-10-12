using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Repositories;

using System.Collections.Generic;
using System.Collections.Immutable;
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
        { TenantId.Bannerlord, [261550] },
        { TenantId.Rimworld, [294100] },
        { TenantId.StardewValley, [413150] },
        { TenantId.Valheim, [892970] },
        { TenantId.Terraria, [105600] },
    };

    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly ISteamAPIClient _steamAPIClient;

    public DatabaseSteamStorage(IUnitOfWorkFactory unitOfWorkFactory, ISteamAPIClient steamAPIClient)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _steamAPIClient = steamAPIClient;
    }

    public async Task<bool> CheckOwnedGamesAsync(NexusModsUserId nexusModsUserId, string steamUserId)
    {
        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite(TenantId.None);

        var nexusModsUserToIntegrationSteam = new NexusModsUserToIntegrationSteamEntity
        {
            NexusModsUserId = nexusModsUserId,
            NexusModsUser = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsUser(nexusModsUserId),
            SteamUserId = steamUserId,
        };

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
                list.Add(new IntegrationSteamToOwnedTenantEntity
                {
                    SteamUserId = steamUserId,
                    OwnedTenant = tenant,
                });
            }
        }

        unitOfWrite.NexusModsUserToSteam.Upsert(nexusModsUserToIntegrationSteam);
        unitOfWrite.IntegrationSteamToOwnedTenants.UpsertRange(list.ToArray());
        unitOfWrite.IntegrationSteamToOwnedTenants.Remove(x => x.SteamUserId == steamUserId);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return true;
    }

    public async Task<Dictionary<string, string>?> GetAsync(string steamUserId)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead(TenantId.None);

        var entity = await unitOfRead.IntegrationSteamTokens.FirstOrDefaultAsync(x => x.SteamUserId.Equals(steamUserId), null, CancellationToken.None);
        if (entity is null) return null;
        return entity.Data;
    }

    public async Task<bool> UpsertAsync(NexusModsUserId nexusModsUserId, string steamUserId, Dictionary<string, string> data)
    {
        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite(TenantId.None);

        var nexusModsUserToIntegrationSteam = new NexusModsUserToIntegrationSteamEntity
        {
            NexusModsUserId = nexusModsUserId,
            NexusModsUser = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsUser(nexusModsUserId),
            SteamUserId = steamUserId,
        };
        var tokensSteam = new IntegrationSteamTokensEntity
        {
            SteamUserId = steamUserId,
            NexusModsUserId = nexusModsUserId,
            NexusModsUser = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsUser(nexusModsUserId),
            Data = data,
        };

        unitOfWrite.NexusModsUserToSteam.Upsert(nexusModsUserToIntegrationSteam);
        unitOfWrite.IntegrationSteamTokens.Upsert(tokensSteam);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return true;
    }

    public async Task<bool> RemoveAsync(NexusModsUserId nexusModsUserId, string steamUserId)
    {
        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite(TenantId.None);

        unitOfWrite.NexusModsUserToSteam.Remove(x => x.NexusModsUser.NexusModsUserId == nexusModsUserId && x.SteamUserId == steamUserId);
        unitOfWrite.IntegrationSteamTokens.Remove(x => x.SteamUserId == steamUserId);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return true;
    }
}