using System;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Contexts;

public interface IAppDbContextSaveScope : IAsyncDisposable
{
    Task CancelAsync();
}