using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Contexts;

public interface IAppDbContextWrite : IAppDbContextRead
{
    EntityFactory GetEntityFactory();

    Task<IAppDbContextSaveScope> CreateSaveScopeAsync();

    Task BulkUpsertAsync<TEntity>(DbSet<TEntity> dbSet, IEnumerable<TEntity> entities) where TEntity : class, IEntity;
    Task BulkSynchronizeAsync<TEntity>(DbSet<TEntity> dbSet, IEnumerable<TEntity> entities) where TEntity : class, IEntity;

    Task BulkUpsertAsync<TEntity>(DbSet<TEntity> dbSet, IAsyncEnumerable<TEntity> entities) where TEntity : class, IEntity;
    Task BulkSynchronizeAsync<TEntity>(DbSet<TEntity> dbSet, IAsyncEnumerable<TEntity> entities) where TEntity : class, IEntity;
}