using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;

using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

public sealed record UserAvailableNexusModsModModel
{
    public required NexusModsModId NexusModsModId { get; init; }
    public required string Name { get; init; }
}

public sealed record UserLinkedNexusModsModModel
{
    public required NexusModsModId NexusModsModId { get; init; }
    public required string Name { get; init; }
    public required NexusModsUserId[] OwnerNexusModsUserIds { get; init; }
    public required NexusModsUserId[] AllowedNexusModsUserIds { get; init; }
    public required NexusModsUserId[] ManuallyLinkedNexusModsUserIds { get; init; }
    public required ModuleId[] ManuallyLinkedModuleIds { get; init; }
    public required ModuleId[] KnownModuleIds { get; init; }
}

public sealed record UserAvailableSteamWorkshopModModel
{
    public required SteamWorkshopModId SteamWorkshopModId { get; init; }
    public required string Name { get; init; }
}

public sealed record UserLinkedSteamWorkshopModModel
{
    public required SteamWorkshopModId SteamWorkshopModId { get; init; }
    public required string Name { get; init; }
    public required NexusModsUserId[] OwnerNexusModsUserIds { get; init; }
    public required NexusModsUserId[] AllowedNexusModsUserIds { get; init; }
    public required NexusModsUserId[] ManuallyLinkedNexusModsUserIds { get; init; }
    public required ModuleId[] ManuallyLinkedModuleIds { get; init; }
    public required ModuleId[] KnownModuleIds { get; init; }
}

public interface INexusModsUserRepositoryRead : IRepositoryRead<NexusModsUserEntity>
{
    Task<NexusModsUserEntity?> GetUserWithIntegrationsAsync(NexusModsUserId userId, CancellationToken ct);
    Task<NexusModsUserEntity> GetUserAsync(NexusModsUserId userId, CancellationToken ct);

    Task<Paging<UserLinkedNexusModsModModel>> GetNexusModsModsPaginatedAsync(NexusModsUserId userId, PaginatedQuery query, CancellationToken ct);
    Task<Paging<UserAvailableNexusModsModModel>> GetAvailableNexusModsModsPaginatedAsync(NexusModsUserId userId, PaginatedQuery query, CancellationToken ct);

    Task<Paging<UserLinkedSteamWorkshopModModel>> GetSteamWorkshopModsPaginatedAsync(NexusModsUserId userId, PaginatedQuery query, CancellationToken ct);
    Task<Paging<UserAvailableSteamWorkshopModModel>> GetAvailableSteamWorkshopModsPaginatedAsync(NexusModsUserId userId, PaginatedQuery query, CancellationToken ct);

    Task<int> GetLinkedNexusModsModCountAsync(NexusModsUserId userId, CancellationToken ct);
}
public interface INexusModsUserRepositoryWrite : IRepositoryWrite<NexusModsUserEntity>, INexusModsUserRepositoryRead;