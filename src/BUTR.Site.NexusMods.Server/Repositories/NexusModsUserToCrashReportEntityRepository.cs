using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface INexusModsUserToCrashReportEntityRepositoryRead : IRepositoryRead<NexusModsUserToCrashReportEntity>;
public interface INexusModsUserToCrashReportEntityRepositoryWrite : IRepositoryWrite<NexusModsUserToCrashReportEntity>, INexusModsUserToCrashReportEntityRepositoryRead;

[ScopedService<INexusModsUserToCrashReportEntityRepositoryWrite, INexusModsUserToCrashReportEntityRepositoryRead>]
internal class NexusModsUserToCrashReportEntityRepository : Repository<NexusModsUserToCrashReportEntity>, INexusModsUserToCrashReportEntityRepositoryWrite
{
    protected override IQueryable<NexusModsUserToCrashReportEntity> InternalQuery => base.InternalQuery
        .Include(x => x.NexusModsUser)
        .Include(x => x.ToCrashReport);

    public NexusModsUserToCrashReportEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }
}