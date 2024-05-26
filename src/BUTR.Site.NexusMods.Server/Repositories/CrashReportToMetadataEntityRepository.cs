using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface ICrashReportToMetadataEntityRepositoryRead : IRepositoryRead<CrashReportToMetadataEntity>;
public interface ICrashReportToMetadataEntityRepositoryWrite : IRepositoryWrite<CrashReportToMetadataEntity>, ICrashReportToMetadataEntityRepositoryRead;

[ScopedService<ICrashReportToMetadataEntityRepositoryWrite, ICrashReportToMetadataEntityRepositoryRead>]
internal class CrashReportToMetadataEntityRepository : Repository<CrashReportToMetadataEntity>, ICrashReportToMetadataEntityRepositoryWrite
{
    protected override IQueryable<CrashReportToMetadataEntity> InternalQuery => base.InternalQuery
        .Include(x => x.ToCrashReport);

    public CrashReportToMetadataEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }
}