using Microsoft.EntityFrameworkCore;

using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Contexts;

public class AppDbContextFactory: IAppDbContextFactory
{
    private readonly IDbContextFactory<AppDbContextWrite> _dbContextFactoryWrite;
    private readonly IDbContextFactory<AppDbContextRead> _dbContextFactoryRead;

    public AppDbContextFactory(IDbContextFactory<AppDbContextWrite> dbContextFactoryWrite, IDbContextFactory<AppDbContextRead> dbContextFactoryRead)
    {
        _dbContextFactoryWrite = dbContextFactoryWrite;
        _dbContextFactoryRead = dbContextFactoryRead;
    }

    public IAppDbContextWrite CreateWrite() => _dbContextFactoryWrite.CreateDbContext();
    public async Task<IAppDbContextWrite> CreateWriteAsync(CancellationToken ct) => await _dbContextFactoryWrite.CreateDbContextAsync(ct);

    public IAppDbContextRead CreateRead() => _dbContextFactoryRead.CreateDbContext();
    public async Task<IAppDbContextRead> CreateReadAsync(CancellationToken ct) => await _dbContextFactoryRead.CreateDbContextAsync(ct);
}