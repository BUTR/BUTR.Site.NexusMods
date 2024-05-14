using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;

using EFCore.BulkExtensions;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

/// <summary>
/// Marker interface for repositories.
/// </summary>
public interface IRepository;

public interface IRepositoryRead<TEntity> : IRepository where TEntity : class, IEntity
{
    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>>? filter,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy,
        CancellationToken ct);
    Task<TProjection?> FirstOrDefaultAsync<TProjection>(
        Expression<Func<TEntity, bool>>? filter,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy,
        Expression<Func<TEntity, TProjection>> projection,
        CancellationToken ct);

    Task<TEntity?> LastOrDefaultAsync(
        Expression<Func<TEntity, bool>>? filter,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy,
        CancellationToken ct);
    Task<TProjection?> LastOrDefaultAsync<TProjection>(
        Expression<Func<TEntity, bool>>? filter,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy,
        Expression<Func<TEntity, TProjection>> projection,
        CancellationToken ct);

    Task<IList<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? filter,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy,
        CancellationToken ct);
    Task<IList<TProjection>> GetAllAsync<TProjection>(
        Expression<Func<TEntity, bool>>? filter,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy,
        Expression<Func<TEntity, TProjection>> projection,
        CancellationToken ct);

    Task<Paging<TEntity>> PaginatedAsync(
        PaginatedQuery query,
        uint maxPageSize = 20,
        Sorting? defaultSorting = default,
        CancellationToken ct = default);

    Task<Paging<TProjection>> PaginatedAsync<TProjection>(
        Expression<Func<TEntity, TProjection>> projection,
        PaginatedQuery query,
        uint maxPageSize = 20,
        Sorting? defaultSorting = default,
        CancellationToken ct = default) where TProjection : class;
}

public interface IRepositoryWrite<TEntity> : IRepositoryRead<TEntity> where TEntity : class, IEntity
{
    void Add(TEntity entity);
    void AddRange(IEnumerable<TEntity> entities);

    void Update(TEntity originalEntity, TEntity currentEntity);

    void Upsert(TEntity entity);
    void UpsertRange(IEnumerable<TEntity> entities);

    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);

    int Remove(Expression<Func<TEntity, bool>> filter);
}

internal abstract class Repository<TEntity> : IRepositoryWrite<TEntity> where TEntity : class, IEntity
{
    protected readonly BaseAppDbContext _dbContext;

    protected Repository(BaseAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    protected virtual IQueryable<TEntity> InternalQuery => _dbContext.Set<TEntity>();

    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>>? filter, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy, CancellationToken ct) => await InternalQuery
        .WhereIf(filter != null, filter!)
        .OrderByIf(orderBy != null, orderBy!)
        .FirstOrDefaultAsync(ct);

    public async Task<TProjection?> FirstOrDefaultAsync<TProjection>(Expression<Func<TEntity, bool>>? filter, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy, Expression<Func<TEntity, TProjection>> projection, CancellationToken ct) => await InternalQuery
        .WhereIf(filter != null, filter!)
        .OrderByIf(orderBy != null, orderBy!)
        .Select(projection)
        .FirstOrDefaultAsync(ct);

    public async Task<TEntity?> LastOrDefaultAsync(Expression<Func<TEntity, bool>>? filter, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy, CancellationToken ct) => await InternalQuery
        .WhereIf(filter != null, filter!)
        .OrderByIf(orderBy != null, orderBy!)
        .LastOrDefaultAsync(ct);

    public async Task<TProjection?> LastOrDefaultAsync<TProjection>(Expression<Func<TEntity, bool>>? filter, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy, Expression<Func<TEntity, TProjection>> projection, CancellationToken ct) => await InternalQuery
        .WhereIf(filter != null, filter!)
        .OrderByIf(orderBy != null, orderBy!)
        .Select(projection)
        .LastOrDefaultAsync(ct);

    public virtual async Task<IList<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? filter, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy, CancellationToken ct = default) => await InternalQuery
        .WhereIf(filter != null, filter!)
        .OrderByIf(orderBy != null, orderBy!)
        .ToListAsync(ct);

    public async Task<IList<TProjection>> GetAllAsync<TProjection>(Expression<Func<TEntity, bool>>? filter, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy, Expression<Func<TEntity, TProjection>> projection, CancellationToken ct) => await InternalQuery
        .WhereIf(filter != null, filter!)
        .OrderByIf(orderBy != null, orderBy!)
        .Select(projection)
        .ToListAsync(ct);

    public Task<Paging<TEntity>> PaginatedAsync(PaginatedQuery query, uint maxPageSize = 20, Sorting? defaultSorting = default, CancellationToken ct = default) => InternalQuery
        .PaginatedAsync(query, maxPageSize, defaultSorting, ct);

    public Task<Paging<TProjection>> PaginatedAsync<TProjection>(Expression<Func<TEntity, TProjection>> projection, PaginatedQuery query, uint maxPageSize = 20, Sorting? defaultSorting = default, CancellationToken ct = default)
        where TProjection : class => InternalQuery
        .Select(projection)
        .PaginatedAsync(query, maxPageSize, defaultSorting, ct);


    public virtual void Add(TEntity entity) => _dbContext.Set<TEntity>()
        .Add(entity);

    public virtual void AddRange(IEnumerable<TEntity> entities) => _dbContext.Set<TEntity>()
        .AddRange(entities);

    public void Update(TEntity originalEntity, TEntity currentEntity)
    {
        _dbContext.UpdateProperties(originalEntity, currentEntity);
    }

    public virtual void Upsert(TEntity entity)
    {
        if (_dbContext is AppDbContextWrite appDbContextWrite)
        {
            appDbContextWrite.AddFutureAction(dbContext => dbContext
                .BulkInsertOrUpdateAsync([entity], o => { o.IncludeGraph = false; }));
        }
    }

    public virtual void UpsertRange(IEnumerable<TEntity> entities)
    {
        if (_dbContext is AppDbContextWrite appDbContextWrite)
        {
            appDbContextWrite.AddFutureAction(dbContext => dbContext
                .BulkInsertOrUpdateAsync(entities, o => { o.IncludeGraph = false; }));
        }
    }

    public virtual void Remove(TEntity entity) => _dbContext.Set<TEntity>()
        .Remove(entity);

    public virtual void RemoveRange(IEnumerable<TEntity> entities) => _dbContext.Set<TEntity>()
        .RemoveRange(entities);

    public int Remove(Expression<Func<TEntity, bool>> filter) => _dbContext.Set<TEntity>()
        .Where(filter)
        .ExecuteDelete();
}