using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Repositories;

[ScopedService<INexusModsUserToIntegrationSteamEntityRepositoryWrite, INexusModsUserToIntegrationSteamEntityRepositoryRead>]
internal class NexusModsUserToIntegrationSteamEntityRepository : Repository<NexusModsUserToIntegrationSteamEntity>, INexusModsUserToIntegrationSteamEntityRepositoryWrite
{
    protected override IQueryable<NexusModsUserToIntegrationSteamEntity> InternalQuery => base.InternalQuery
        .Include(x => x.NexusModsUser)
        .Include(x => x.ToTokens)
        .Include(x => x.ToOwnedTenants);

    public NexusModsUserToIntegrationSteamEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }
}