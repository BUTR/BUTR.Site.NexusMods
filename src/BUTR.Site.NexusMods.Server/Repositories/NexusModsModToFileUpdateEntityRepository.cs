using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Repositories;

[ScopedService<INexusModsModToFileUpdateEntityRepositoryWrite, INexusModsModToFileUpdateEntityRepositoryRead>]
internal class NexusModsModToFileUpdateEntityRepository : Repository<NexusModsModToFileUpdateEntity>, INexusModsModToFileUpdateEntityRepositoryWrite
{
    protected override IQueryable<NexusModsModToFileUpdateEntity> InternalQuery => base.InternalQuery
        .Include(x => x.NexusModsMod).ThenInclude(x => x.Name);

    public NexusModsModToFileUpdateEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }
}