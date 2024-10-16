using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Repositories;

[ScopedService<ISteamWorkshopModToNameEntityRepositoryWrite, ISteamWorkshopModToNameEntityRepositoryRead>]
internal class SteamWorkshopModToNameEntityRepository : Repository<SteamWorkshopModToNameEntity>, ISteamWorkshopModToNameEntityRepositoryWrite
{
    protected override IQueryable<SteamWorkshopModToNameEntity> InternalQuery => base.InternalQuery
        .Include(x => x.SteamWorkshopMod).ThenInclude(x => x.Name);

    public SteamWorkshopModToNameEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }
}