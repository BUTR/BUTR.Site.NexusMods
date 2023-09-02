using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Contexts;

public interface IAppDbContextWrite : IAppDbContextRead
{
    EntityFactory CreateEntityFactory();
    
    IAsyncDisposable CreateSaveScope();

    void FutureUpsert<TEntity>(Func<IAppDbContextWrite, DbSet<TEntity>> dbset, TEntity entity) where TEntity : class, IEntity;
    void FutureUpsert<TEntity>(Func<IAppDbContextWrite, DbSet<TEntity>> dbset, ICollection<TEntity> entities) where TEntity : class, IEntity;
    void FutureSyncronize<TEntity>(Func<IAppDbContextWrite, DbSet<TEntity>> dbset, ICollection<TEntity> entities) where TEntity : class, IEntity;

    Task SaveAsync(CancellationToken ct = default);
}