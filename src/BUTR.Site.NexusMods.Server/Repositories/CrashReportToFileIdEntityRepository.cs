using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Repositories;

[ScopedService<ICrashReportToFileIdEntityRepositoryWrite, ICrashReportToFileIdEntityRepositoryRead>]
internal class CrashReportToFileIdEntityRepository : Repository<CrashReportToFileIdEntity>, ICrashReportToFileIdEntityRepositoryWrite
{
    protected override IQueryable<CrashReportToFileIdEntity> InternalQuery => base.InternalQuery
        .Include(x => x.ToCrashReport);

    public CrashReportToFileIdEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }
}