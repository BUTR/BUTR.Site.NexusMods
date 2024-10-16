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

[ScopedService<INexusModsUserToSteamWorkshopModEntityRepositoryWrite, INexusModsUserToSteamWorkshopModEntityRepositoryRead>]
internal class NexusModsUserToSteamWorkshopModEntityRepository : Repository<NexusModsUserToSteamWorkshopModEntity>, INexusModsUserToSteamWorkshopModEntityRepositoryWrite
{
    protected override IQueryable<NexusModsUserToSteamWorkshopModEntity> InternalQuery => base.InternalQuery
        .Include(x => x.NexusModsUser)
        .Include(x => x.SteamWorkshopMod);

    public NexusModsUserToSteamWorkshopModEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }

    public async Task<Paging<UserManuallyLinkedSteamWorkshopModModel>> GetManuallyLinkedPaginatedAsync(NexusModsUserId userId, PaginatedQuery query, CancellationToken ct) => await _dbContext.NexusModsUserToSteamWorkshopMods
        .Include(x => x.NexusModsUser).ThenInclude(x => x.Name)
        .Where(x => x.NexusModsUserId == userId && x.LinkType == NexusModsUserToModLinkType.ByOwner)
        .GroupBy(x => new { x.SteamWorkshopModId })
        .Select(x => new UserManuallyLinkedSteamWorkshopModModel
        {
            SteamWorkshopModId = x.Key.SteamWorkshopModId,
            NexusModsUsers = x.Select(y => new UserManuallyLinkedModUserModel
            {
                NexusModsUserId = y.NexusModsUserId,
                NexusModsUsername = y.NexusModsUser.Name!.Name,
            }).ToArray(),
        })
        .PaginatedAsync(query, 20, new() { Property = nameof(UserManuallyLinkedSteamWorkshopModModel.SteamWorkshopModId), Type = SortingType.Ascending }, ct);
}