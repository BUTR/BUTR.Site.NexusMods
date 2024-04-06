using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Extensions;

public static class DbSetExtensions
{
    public static async Task UpsertOnSaveAsync<TEntity>(this DbSet<TEntity> dbSet, IEnumerable<TEntity> entities) where TEntity : class, IEntity
    {
        if (dbSet.GetService<ICurrentDbContext>().Context is IAppDbContextWrite dbContext)
            await dbContext.BulkUpsertAsync(dbSet, entities);
    }
    public static async Task UpsertOnSaveAsync<TEntity>(this DbSet<TEntity> dbSet, IAsyncEnumerable<TEntity> entities) where TEntity : class, IEntity
    {
        if (dbSet.GetService<ICurrentDbContext>().Context is IAppDbContextWrite dbContext)
            await dbContext.BulkUpsertAsync(dbSet, entities);
    }
    public static async Task UpsertOnSaveAsync<TEntity>(this DbSet<TEntity> dbSet, params TEntity[] entities) where TEntity : class, IEntity
    {
        if (dbSet.GetService<ICurrentDbContext>().Context is IAppDbContextWrite dbContext)
            await dbContext.BulkUpsertAsync(dbSet, entities);
    }

    public static async Task SynchronizeOnSaveAsync<TEntity>(this DbSet<TEntity> dbSet, IEnumerable<TEntity> entities) where TEntity : class, IEntity
    {
        if (dbSet.GetService<ICurrentDbContext>().Context is IAppDbContextWrite dbContext)
            await dbContext.BulkSynchronizeAsync(dbSet, entities);
    }
    public static async Task SynchronizeOnSaveAsync<TEntity>(this DbSet<TEntity> dbSet, IAsyncEnumerable<TEntity> entities) where TEntity : class, IEntity
    {
        if (dbSet.GetService<ICurrentDbContext>().Context is IAppDbContextWrite dbContext)
            await dbContext.BulkSynchronizeAsync(dbSet, entities);
    }
    public static async Task SynchronizeOnSaveAsync<TEntity>(this DbSet<TEntity> dbSet, params TEntity[] entities) where TEntity : class, IEntity
    {
        if (dbSet.GetService<ICurrentDbContext>().Context is IAppDbContextWrite dbContext)
            await dbContext.BulkSynchronizeAsync(dbSet, entities);
    }


    public static async Task UpsertAsync<TEntity>(this DbSet<TEntity> dbSet, IEnumerable<TEntity> entities, bool @unsafe = false) where TEntity : class, IEntity
    {
        await dbSet.BulkMergeAsync(entities, o => { o.UnsafeMode = @unsafe; o.UseInternalTransaction = true; });
    }
    public static async Task UpsertAsync<TEntity>(this DbSet<TEntity> dbSet, IAsyncEnumerable<TEntity> entities, bool @unsafe = false) where TEntity : class, IEntity
    {
        await dbSet.BulkMergeAsync(await entities.ToArrayAsync(), o => { o.UnsafeMode = @unsafe; o.UseInternalTransaction = true; });
    }
    public static async Task UpsertAsync<TEntity>(this DbSet<TEntity> dbSet, bool @unsafe = false, params TEntity[] entities) where TEntity : class, IEntity
    {
        await dbSet.BulkMergeAsync(entities, o => { o.UnsafeMode = @unsafe; o.UseInternalTransaction = true; });
    }

    public static async Task SynchronizeAsync<TEntity>(this DbSet<TEntity> dbSet, IEnumerable<TEntity> entities) where TEntity : class, IEntity
    {
        await dbSet.BulkSynchronizeAsync(entities, o => { o.UseInternalTransaction = true; });
    }
    public static async Task SynchronizeAsync<TEntity>(this DbSet<TEntity> dbSet, IAsyncEnumerable<TEntity> entities) where TEntity : class, IEntity
    {
        await dbSet.BulkSynchronizeAsync(await entities.ToArrayAsync(), o => { o.UseInternalTransaction = true; });
    }
    public static async Task SynchronizeAsync<TEntity>(this DbSet<TEntity> dbSet, params TEntity[] entities) where TEntity : class, IEntity
    {
        await dbSet.BulkSynchronizeAsync(entities, o => { o.UseInternalTransaction = true; });
    }
}