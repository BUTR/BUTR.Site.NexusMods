using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Contexts;

public sealed class AppContextSaveScope : IAsyncDisposable
{
    private readonly IAppDbContextWrite _dbContextWrite;
    private readonly EntityFactory _entityFactory;

    public AppContextSaveScope(IAppDbContextWrite dbContextWrite, EntityFactory entityFactory)
    {
        _dbContextWrite = dbContextWrite;
        _entityFactory = entityFactory;
    }

    public async ValueTask DisposeAsync()
    {
        await _entityFactory.SaveCreated(CancellationToken.None);
        await _dbContextWrite.SaveAsync(CancellationToken.None);
    }
}