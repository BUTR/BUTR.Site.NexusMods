using BUTR.Site.NexusMods.Server.Models.Database;

using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface IQuartzExecutionLogEntityRepositoryRead : IRepositoryRead<QuartzExecutionLogEntity>;

public interface IQuartzExecutionLogEntityRepositoryWrite : IRepositoryWrite<QuartzExecutionLogEntity>, IQuartzExecutionLogEntityRepositoryRead
{
    Task MarkIncompleteAsync(CancellationToken ct);
}