using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;

using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

public sealed record UserManuallyLinkedSteamWorkshopModModel
{
    public required SteamWorkshopModId SteamWorkshopModId { get; init; }
    public required UserManuallyLinkedModUserModel[] NexusModsUsers { get; init; }
}

public interface INexusModsUserToSteamWorkshopModEntityRepositoryRead : IRepositoryRead<NexusModsUserToSteamWorkshopModEntity>
{
    Task<Paging<UserManuallyLinkedSteamWorkshopModModel>> GetManuallyLinkedPaginatedAsync(NexusModsUserId userId, PaginatedQuery query, CancellationToken ct);
}

public interface INexusModsUserToSteamWorkshopModEntityRepositoryWrite : IRepositoryWrite<NexusModsUserToSteamWorkshopModEntity>, INexusModsUserToSteamWorkshopModEntityRepositoryRead;