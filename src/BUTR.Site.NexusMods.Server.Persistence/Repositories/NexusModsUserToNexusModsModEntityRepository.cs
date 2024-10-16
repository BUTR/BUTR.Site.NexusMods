using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;

using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

public sealed record UserManuallyLinkedModUserModel
{
    public required NexusModsUserId NexusModsUserId { get; init; }
    public required NexusModsUserName NexusModsUsername { get; init; }
}
public sealed record UserManuallyLinkedNexusModsModModel
{
    public required NexusModsModId NexusModsModId { get; init; }
    public required UserManuallyLinkedModUserModel[] NexusModsUsers { get; init; }
}

public interface INexusModsUserToNexusModsModEntityRepositoryRead : IRepositoryRead<NexusModsUserToNexusModsModEntity>
{
    Task<Paging<UserManuallyLinkedNexusModsModModel>> GetManuallyLinkedPaginatedAsync(NexusModsUserId userId, PaginatedQuery query, CancellationToken ct);
}
public interface INexusModsUserToNexusModsModEntityRepositoryWrite : IRepositoryWrite<NexusModsUserToNexusModsModEntity>, INexusModsUserToNexusModsModEntityRepositoryRead;