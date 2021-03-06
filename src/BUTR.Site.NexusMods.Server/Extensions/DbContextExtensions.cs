using BUTR.Site.NexusMods.Server.Utils.Npgsql;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Extensions
{
    public static class DbContextExtensions
    {
        public static void AddUpdateRemove<TEntity>(this DbContext dbContext, Expression<Func<TEntity, bool>> predicate, Func<TEntity?, TEntity?> applyChanges) where TEntity : class
        {
            AddUpdateRemove(dbContext, dbContext.Set<TEntity>(), predicate, applyChanges);
        }
        public static void AddUpdateRemove<TEntity>(this DbContext dbContext, IQueryable<TEntity> dbSet, Expression<Func<TEntity, bool>> predicate, Func<TEntity?, TEntity?> applyChanges) where TEntity : class
        {
            var entity = dbSet.FirstOrDefault(predicate);
            var changed = applyChanges(entity);

            if (changed is null)
            {
                if (entity is not null) dbContext.Entry(entity).State = EntityState.Deleted;
                return;
            }

            var changeTrackerEntity = dbContext.Entry(entity ?? changed);
            changeTrackerEntity.State = entity is null ? EntityState.Added : EntityState.Modified;
            changeTrackerEntity.CurrentValues.SetValues(changed);
        }

        public static Task AddUpdateRemoveAsync<TEntity>(this DbContext dbContext, Expression<Func<TEntity, bool>> predicate, Func<TEntity?, TEntity?> applyChanges, CancellationToken ct = default) where TEntity : class
        {
            return AddUpdateRemoveAsync(dbContext, dbContext.Set<TEntity>(), predicate, applyChanges, ct);
        }
        public static async Task AddUpdateRemoveAsync<TEntity>(this DbContext dbContext, IQueryable<TEntity> dbSet, Expression<Func<TEntity, bool>> predicate, Func<TEntity?, TEntity?> applyChanges, CancellationToken ct = default) where TEntity : class
        {
            var entity = await dbSet.FirstOrDefaultAsync(predicate, ct);
            var changed = applyChanges(entity);

            if (changed is null)
            {
                if (entity is not null) dbContext.Entry(entity).State = EntityState.Deleted;
                return;
            }

            var changeTrackerEntity = dbContext.Entry(entity ?? changed);
            changeTrackerEntity.State = entity is null ? EntityState.Added : EntityState.Modified;
            changeTrackerEntity.CurrentValues.SetValues(changed);
        }

        public static async Task<bool> AddUpdateRemoveAndSaveAsync<TEntity>(this DbContext dbContext, IQueryable<TEntity> dbSet, Expression<Func<TEntity, bool>> predicate, Func<TEntity?, TEntity?> applyChanges, CancellationToken ct = default) where TEntity : class
        {
            await dbContext.AddUpdateRemoveAsync(dbSet, predicate, applyChanges, ct);
            return await dbContext.SaveChangesAsync(ct) > 0;
        }

        public static async Task<bool> AddUpdateRemoveAndSaveAsync<TEntity>(this DbContext dbContext, Expression<Func<TEntity, bool>> predicate, Func<TEntity?, TEntity?> applyChanges, CancellationToken ct = default) where TEntity : class
        {
            await dbContext.AddUpdateRemoveAsync(predicate, applyChanges, ct);
            return await dbContext.SaveChangesAsync(ct) > 0;
        }


        public static TEntity? FirstOrDefault<TEntity>(this DbContext dbContext, Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            return dbContext.Set<TEntity>().FirstOrDefault(predicate);
        }

        public static Task<TEntity?> FirstOrDefaultAsync<TEntity>(this DbContext dbContext, Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) where TEntity : class
        {
            return dbContext.Set<TEntity>().FirstOrDefaultAsync(predicate, ct);
        }

        public static IQueryable<T> Prepare<T>(this IQueryable<T> query) => query.TagWith(PrepareCommandInterceptor.Tag);
    }
}