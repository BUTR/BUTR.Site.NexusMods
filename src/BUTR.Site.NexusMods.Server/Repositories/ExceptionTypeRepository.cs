using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Repositories;

[ScopedService<IExceptionTypeRepositoryWrite, IExceptionTypeRepositoryRead>]
internal class ExceptionTypeRepository : Repository<ExceptionTypeEntity>, IExceptionTypeRepositoryWrite
{
    protected override IQueryable<ExceptionTypeEntity> InternalQuery => base.InternalQuery
    //.Include(x => x.ToCrashReports)
    //.Include(x => x.ToTopExceptionsTypes)
    ;

    public ExceptionTypeRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }
}