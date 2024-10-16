using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Repositories;

[ScopedService<ISteamWorkshopModToFileUpdateEntityRepositoryWrite, ISteamWorkshopModToFileUpdateEntityRepositoryRead>]
internal class SteamWorkshopModToFileUpdateEntityRepository : Repository<SteamWorkshopModToFileUpdateEntity>, ISteamWorkshopModToFileUpdateEntityRepositoryWrite
{
    protected override IQueryable<SteamWorkshopModToFileUpdateEntity> InternalQuery => base.InternalQuery
        .Include(x => x.SteamWorkshopMod).ThenInclude(x => x.Name);

    public SteamWorkshopModToFileUpdateEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }
}