using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;

using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

public sealed record UserAvailableModModel
{
    public required NexusModsModId NexusModsModId { get; init; }
    public required string Name { get; init; }
}

public sealed record UserLinkedModModel
{
    public required NexusModsModId NexusModsModId { get; init; }
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

    Task<Paging<UserLinkedModModel>> GetNexusModsModsPaginatedAsync(NexusModsUserId userId, PaginatedQuery query, CancellationToken ct);

    Task<Paging<UserAvailableModModel>> GetAvailableModsPaginatedAsync(NexusModsUserId userId, PaginatedQuery query, CancellationToken ct);

    Task<int> GetLinkedModCountAsync(NexusModsUserId userId, CancellationToken ct);
}
public interface INexusModsUserRepositoryWrite : IRepositoryWrite<NexusModsUserEntity>, INexusModsUserRepositoryRead;