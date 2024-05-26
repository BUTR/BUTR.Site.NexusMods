using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface ICrashReportIgnoredFileEntityRepositoryRead : IRepositoryRead<CrashReportIgnoredFileEntity>;
public interface ICrashReportIgnoredFileEntityRepositoryWrite : IRepositoryWrite<CrashReportIgnoredFileEntity>, ICrashReportIgnoredFileEntityRepositoryRead;

[ScopedService<ICrashReportIgnoredFileEntityRepositoryWrite, ICrashReportIgnoredFileEntityRepositoryRead>]
internal class CrashReportIgnoredFileEntityRepository : Repository<CrashReportIgnoredFileEntity>, ICrashReportIgnoredFileEntityRepositoryWrite
{
    protected override IQueryable<CrashReportIgnoredFileEntity> InternalQuery => base.InternalQuery;

    public CrashReportIgnoredFileEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }
}