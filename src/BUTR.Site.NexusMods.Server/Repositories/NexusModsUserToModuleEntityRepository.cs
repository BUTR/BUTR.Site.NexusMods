using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

[ScopedService<INexusModsUserToModuleEntityRepositoryWrite, INexusModsUserToModuleEntityRepositoryRead>]
internal class NexusModsUserToModuleEntityRepository : Repository<NexusModsUserToModuleEntity>, INexusModsUserToModuleEntityRepositoryWrite
{
    protected override IQueryable<NexusModsUserToModuleEntity> InternalQuery => base.InternalQuery
        .Include(x => x.NexusModsUser)
        .Include(x => x.Module);

    public NexusModsUserToModuleEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }

    public async Task<Paging<UserManuallyLinkedModuleModel>> GetManuallyLinkedModuleIdsPaginatedAsync(PaginatedQuery query, NexusModsUserToModuleLinkType linkType, CancellationToken ct)
    {
        var unknown = NexusModsUserName.From("UNKNOWN");
        return await _dbContext.NexusModsUserToModules
            .Include(x => x.NexusModsUser).ThenInclude(x => x.Name)
            .Where(x => x.LinkType == linkType)
            .GroupBy(x => new { x.NexusModsUser.NexusModsUserId, x.NexusModsUser.Name!.Name })
            .Select(x => new UserManuallyLinkedModuleModel
            {
                NexusModsUserId = x.Key.NexusModsUserId,
                NexusModsUsername = x.Select(y => y.NexusModsUser.Name == null ? unknown : y.NexusModsUser.Name.Name).First(),
                ModuleIds = x.Select(y => y.Module.ModuleId).ToArray(),
            })
            .PaginatedAsync(query, 20, new() { Property = nameof(NexusModsUserEntity.NexusModsUserId), Type = SortingType.Ascending }, ct);
    }
}