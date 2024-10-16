using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Repositories;
using BUTR.Site.NexusMods.Shared.Helpers;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public interface ISteamStorage
{
    Task<bool> CheckOwnedGamesAsync(NexusModsUserId nexusModsUserId, SteamUserId steamUserId);

    Task<Dictionary<string, string>?> GetAsync(SteamUserId steamUserId);
    Task<bool> UpsertAsync(NexusModsUserId nexusModsUserId, SteamUserId steamUserId, Dictionary<string, string> data);
    Task<bool> RemoveAsync(NexusModsUserId nexusModsUserId, SteamUserId steamUserId);
}

[ScopedService<ISteamStorage>]
public sealed class DatabaseSteamStorage : ISteamStorage
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly ISteamAPIClient _steamAPIClient;

    public DatabaseSteamStorage(IUnitOfWorkFactory unitOfWorkFactory, ISteamAPIClient steamAPIClient)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _steamAPIClient = steamAPIClient;
    }

    public async Task<bool> CheckOwnedGamesAsync(NexusModsUserId nexusModsUserId, SteamUserId steamUserId)
    {
        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite(TenantId.None);

        var nexusModsUserToIntegrationSteam = new NexusModsUserToIntegrationSteamEntity
        {
            NexusModsUserId = nexusModsUserId,
            NexusModsUser = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsUser(nexusModsUserId),
            SteamUserId = steamUserId,
        };

        var list = ImmutableArray.CreateBuilder<IntegrationSteamToOwnedTenantEntity>();
        foreach (var (tenant, gameIds) in TenantId.Values.Select(x => (x, TenantUtils.FromTenantToSteamAppIds(x.Value))))
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

    public async Task<Dictionary<string, string>?> GetAsync(SteamUserId steamUserId)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead(TenantId.None);

        var entity = await unitOfRead.IntegrationSteamTokens.FirstOrDefaultAsync(x => x.SteamUserId.Equals(steamUserId), null, CancellationToken.None);
        if (entity is null) return null;
        return entity.Data;
    }

    public async Task<bool> UpsertAsync(NexusModsUserId nexusModsUserId, SteamUserId steamUserId, Dictionary<string, string> data)
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

    public async Task<bool> RemoveAsync(NexusModsUserId nexusModsUserId, SteamUserId steamUserId)
    {
        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite(TenantId.None);

        unitOfWrite.NexusModsUserToSteam.Remove(x => x.NexusModsUserId == nexusModsUserId && x.SteamUserId == steamUserId);
        unitOfWrite.IntegrationSteamTokens.Remove(x => x.SteamUserId == steamUserId);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return true;
    }
}