using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;

using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

public sealed record UserManuallyLinkedModuleModel
{
    public required NexusModsUserId NexusModsUserId { get; init; }
    public required NexusModsUserName NexusModsUsername { get; init; }
    public required ModuleId[] ModuleIds { get; init; }
}

public interface INexusModsUserToModuleEntityRepositoryRead : IRepositoryRead<NexusModsUserToModuleEntity>
{
    Task<Paging<UserManuallyLinkedModuleModel>> GetManuallyLinkedModuleIdsPaginatedAsync(PaginatedQuery query, NexusModsUserToModuleLinkType linkType, CancellationToken ct);
}
public interface INexusModsUserToModuleEntityRepositoryWrite : IRepositoryWrite<NexusModsUserToModuleEntity>, INexusModsUserToModuleEntityRepositoryRead;