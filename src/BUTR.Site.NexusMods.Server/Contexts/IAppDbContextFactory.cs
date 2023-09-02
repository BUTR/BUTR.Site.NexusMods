using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Contexts;

public interface IAppDbContextFactory
{
    IAppDbContextWrite CreateWrite();
    Task<IAppDbContextWrite> CreateWriteAsync(CancellationToken ct = default);

    IAppDbContextRead CreateRead();
    Task<IAppDbContextRead> CreateReadAsync(CancellationToken ct = default);
}