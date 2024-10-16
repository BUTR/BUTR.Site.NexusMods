using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Repositories;

[ScopedService<INexusModsUserToSteamWorkshopModEntityRepositoryWrite, INexusModsUserToSteamWorkshopModEntityRepositoryRead>]
internal class NexusModsUserToSteamWorkshopModEntityRepository : Repository<NexusModsUserToSteamWorkshopModEntity>, INexusModsUserToSteamWorkshopModEntityRepositoryWrite
{
    protected override IQueryable<NexusModsUserToSteamWorkshopModEntity> InternalQuery => base.InternalQuery
        .Include(x => x.NexusModsUser)
        .Include(x => x.SteamWorkshopMod);

    public NexusModsUserToSteamWorkshopModEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }
}