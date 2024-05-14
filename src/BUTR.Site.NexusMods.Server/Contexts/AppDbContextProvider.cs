using BUTR.Site.NexusMods.DependencyInjection;

using System;

namespace BUTR.Site.NexusMods.Server.Contexts;

[ScopedService<IAppDbContextProvider>]
internal sealed class AppDbContextProvider : IAppDbContextProvider
{
    private BaseAppDbContext _current = default!;

    public void Set(BaseAppDbContext dbContext) => _current = dbContext;

    public BaseAppDbContext Get() => _current ?? throw new InvalidOperationException("AppDbContext has not been set.");
}