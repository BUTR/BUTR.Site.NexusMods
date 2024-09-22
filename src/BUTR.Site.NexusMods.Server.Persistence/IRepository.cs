using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;

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