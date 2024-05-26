using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface INexusModsModToModuleInfoHistoryEntityRepositoryRead : IRepositoryRead<NexusModsModToModuleInfoHistoryEntity>;
public interface INexusModsModToModuleInfoHistoryEntityRepositoryWrite : IRepositoryWrite<NexusModsModToModuleInfoHistoryEntity>, INexusModsModToModuleInfoHistoryEntityRepositoryRead;

[ScopedService<INexusModsModToModuleInfoHistoryEntityRepositoryWrite, INexusModsModToModuleInfoHistoryEntityRepositoryRead>]
internal class NexusModsModToModuleInfoHistoryEntityRepository : Repository<NexusModsModToModuleInfoHistoryEntity>, INexusModsModToModuleInfoHistoryEntityRepositoryWrite
{
    protected override IQueryable<NexusModsModToModuleInfoHistoryEntity> InternalQuery => base.InternalQuery
        .Include(x => x.NexusModsMod).ThenInclude(x => x.Name)
        .Include(x => x.Module);

    public NexusModsModToModuleInfoHistoryEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }
}