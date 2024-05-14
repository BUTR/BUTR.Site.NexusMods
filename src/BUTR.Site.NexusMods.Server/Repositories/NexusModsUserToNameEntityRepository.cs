using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface INexusModsUserToNameEntityRepositoryRead : IRepositoryRead<NexusModsUserToNameEntity>;
public interface INexusModsUserToNameEntityRepositoryWrite : IRepositoryWrite<NexusModsUserToNameEntity>, INexusModsUserToNameEntityRepositoryRead;

[ScopedService<INexusModsUserToNameEntityRepositoryWrite, INexusModsUserToNameEntityRepositoryRead>]
internal class NexusModsUserToNameEntityRepository : Repository<NexusModsUserToNameEntity>, INexusModsUserToNameEntityRepositoryWrite
{
    protected override IQueryable<NexusModsUserToNameEntity> InternalQuery => base.InternalQuery
        .Include(x => x.NexusModsUser);

    public NexusModsUserToNameEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }
}