using BUTR.Site.NexusMods.Server.Models.Database;

using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface IStatisticsTopExceptionsTypeEntityRepositoryRead : IRepositoryRead<StatisticsTopExceptionsTypeEntity>;
public interface IStatisticsTopExceptionsTypeEntityRepositoryWrite : IRepositoryWrite<StatisticsTopExceptionsTypeEntity>, IStatisticsTopExceptionsTypeEntityRepositoryRead
{
    Task CalculateAsync(CancellationToken ct);
}