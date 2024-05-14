using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface INexusModsUserToIntegrationGOGEntityRepositoryRead : IRepositoryRead<NexusModsUserToIntegrationGOGEntity>;
public interface INexusModsUserToIntegrationGOGEntityRepositoryWrite : IRepositoryWrite<NexusModsUserToIntegrationGOGEntity>, INexusModsUserToIntegrationGOGEntityRepositoryRead;

[ScopedService<INexusModsUserToIntegrationGOGEntityRepositoryWrite, INexusModsUserToIntegrationGOGEntityRepositoryRead>]
internal class NexusModsUserToIntegrationGOGEntityRepository : Repository<NexusModsUserToIntegrationGOGEntity>, INexusModsUserToIntegrationGOGEntityRepositoryWrite
{
    protected override IQueryable<NexusModsUserToIntegrationGOGEntity> InternalQuery => base.InternalQuery
        .Include(x => x.NexusModsUser)
        .Include(x => x.ToTokens)
        .Include(x => x.ToOwnedTenants);

    public NexusModsUserToIntegrationGOGEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }
}