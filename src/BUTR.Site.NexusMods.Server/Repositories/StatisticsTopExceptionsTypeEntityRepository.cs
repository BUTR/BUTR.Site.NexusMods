using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface IStatisticsTopExceptionsTypeEntityRepositoryRead : IRepositoryRead<StatisticsTopExceptionsTypeEntity>;
public interface IStatisticsTopExceptionsTypeEntityRepositoryWrite : IRepositoryWrite<StatisticsTopExceptionsTypeEntity>, IStatisticsTopExceptionsTypeEntityRepositoryRead;

[ScopedService<IStatisticsTopExceptionsTypeEntityRepositoryWrite, IStatisticsTopExceptionsTypeEntityRepositoryRead>]
internal class StatisticsTopExceptionsTypeEntityRepository : Repository<StatisticsTopExceptionsTypeEntity>, IStatisticsTopExceptionsTypeEntityRepositoryWrite
{
    protected override IQueryable<StatisticsTopExceptionsTypeEntity> InternalQuery => base.InternalQuery
        .Include(x => x.ExceptionType);

    public StatisticsTopExceptionsTypeEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }
}