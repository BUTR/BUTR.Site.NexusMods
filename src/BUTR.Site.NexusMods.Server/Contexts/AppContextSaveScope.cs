using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Contexts;

public sealed class AppContextSaveScope : IAsyncDisposable
{
    private readonly IAppDbContextWrite _dbContextWrite;

    public AppContextSaveScope(IAppDbContextWrite dbContextWrite)
    {
        _dbContextWrite = dbContextWrite;
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContextWrite.SaveAsync(CancellationToken.None);
    }
}