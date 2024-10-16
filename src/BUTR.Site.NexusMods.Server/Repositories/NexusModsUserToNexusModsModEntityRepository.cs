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

[ScopedService<INexusModsUserToNexusModsModEntityRepositoryWrite, INexusModsUserToNexusModsModEntityRepositoryRead>]
internal class NexusModsUserToNexusModsModEntityRepository : Repository<NexusModsUserToNexusModsModEntity>, INexusModsUserToNexusModsModEntityRepositoryWrite
{
    protected override IQueryable<NexusModsUserToNexusModsModEntity> InternalQuery => base.InternalQuery
        .Include(x => x.NexusModsUser)
        .Include(x => x.NexusModsMod);

    public NexusModsUserToNexusModsModEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }

    public async Task<Paging<UserManuallyLinkedNexusModsModModel>> GetManuallyLinkedPaginatedAsync(NexusModsUserId userId, PaginatedQuery query, CancellationToken ct) => await _dbContext.NexusModsUserToNexusModsMods
        .Include(x => x.NexusModsUser).ThenInclude(x => x.Name)
        .Where(x => x.NexusModsUserId == userId && x.LinkType == NexusModsUserToModLinkType.ByOwner)
        .GroupBy(x => new { x.NexusModsModId })
        .Select(x => new UserManuallyLinkedNexusModsModModel
        {
            NexusModsModId = x.Key.NexusModsModId,
            NexusModsUsers = x.Select(y => new UserManuallyLinkedModUserModel
            {
                NexusModsUserId = y.NexusModsUserId,
                NexusModsUsername = y.NexusModsUser.Name!.Name,
            }).ToArray(),
        })
        .PaginatedAsync(query, 20, new() { Property = nameof(NexusModsModEntity.NexusModsModId), Type = SortingType.Ascending }, ct);
}