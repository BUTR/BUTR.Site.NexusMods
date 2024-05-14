using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface INexusModsModToNameEntityRepositoryRead : IRepositoryRead<NexusModsModToNameEntity>;
public interface INexusModsModToNameEntityRepositoryWrite : IRepositoryWrite<NexusModsModToNameEntity>, INexusModsModToNameEntityRepositoryRead;

[ScopedService<INexusModsModToNameEntityRepositoryWrite, INexusModsModToNameEntityRepositoryRead>]

internal class NexusModsModToNameEntityRepository : Repository<NexusModsModToNameEntity>, INexusModsModToNameEntityRepositoryWrite
{
    protected override IQueryable<NexusModsModToNameEntity> InternalQuery => base.InternalQuery
        .Include(x => x.NexusModsMod).ThenInclude(x => x.Name);

    public NexusModsModToNameEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }
}