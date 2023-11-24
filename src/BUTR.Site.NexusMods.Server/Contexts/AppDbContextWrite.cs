using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Contexts;

public sealed class AppDbContextWrite : BaseAppDbContext, IAppDbContextWrite
{
    private sealed class AppDbContextSaveScope : IAppDbContextSaveScope
    {
        public static AppDbContextSaveScope Create(AppDbContextWrite dbContextWrite, Action onDispose) => new(dbContextWrite, onDispose);

        private readonly AppDbContextWrite _dbContextWrite;
        private readonly Action _onDispose;
        private bool _hasCancelled;

        private AppDbContextSaveScope(AppDbContextWrite dbContextWrite, Action onDispose)
        {
            _dbContextWrite = dbContextWrite;
            _onDispose = onDispose;
        }

        public Task CancelAsync()
        {
            if (!_hasCancelled) _hasCancelled = true;

            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (!_hasCancelled)
                {
                    await _dbContextWrite.SaveAsync(CancellationToken.None);
                }
            }
            finally
            {
                _onDispose();
            }
        }
    }

    private readonly ITenantContextAccessor _tenantContextAccessor;
    
    private EntityFactory? _entityFactory;
    private List<Func<Task>>? _onSave;

    public AppDbContextWrite(
        ITenantContextAccessor tenantContextAccessor,
        NpgsqlDataSourceProvider dataSourceProvider,
        IEntityConfigurationFactory entityConfigurationFactory,
        DbContextOptions<AppDbContextWrite> options) : base(dataSourceProvider, entityConfigurationFactory, options)
    {
        _tenantContextAccessor = tenantContextAccessor;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);

        base.OnConfiguring(optionsBuilder);
    }


    public AppDbContextWrite New() => this.GetService<IDbContextFactory<AppDbContextWrite>>().CreateDbContext();

    public EntityFactory GetEntityFactory() => _entityFactory ??= new EntityFactory(_tenantContextAccessor, this);

    public Task<IAppDbContextSaveScope> CreateSaveScopeAsync()
    {
        _onSave = new();
        return Task.FromResult<IAppDbContextSaveScope>(AppDbContextSaveScope.Create(this, () => _onSave = null));
    }

    public async Task SaveAsync(CancellationToken ct)
    {
        if (!ChangeTracker.HasChanges() && _entityFactory is null && _onSave?.Count == 0)
            return;

        var executionStrategy = Database.CreateExecutionStrategy();
        await executionStrategy.ExecuteAsync(this, static async (dbContext, ct) =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                if (dbContext._entityFactory is not null)
                    await dbContext._entityFactory.SaveCreatedAsync(ct);

                foreach (var func in dbContext._onSave ?? Enumerable.Empty<Func<Task>>())
                    await func();

                await dbContext.BulkSaveChangesAsync(o =>
                {
                    o.IncludeGraph = false;
                    o.LegacyIncludeGraph = false;
                }, CancellationToken.None);

                await transaction.CommitAsync(ct);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }, ct);
    }

    public Task BulkUpsertAsync<TEntity>(DbSet<TEntity> dbSet, IEnumerable<TEntity> entities) where TEntity : class, IEntity
    {
        if (_onSave is null) throw new Exception();
        if (!ReferenceEquals(dbSet.GetService<ICurrentDbContext>().Context, this)) throw new Exception();

        _onSave.Add(async () => await dbSet.UpsertAsync(entities));
        return Task.CompletedTask;
    }

    public Task BulkSynchronizeAsync<TEntity>(DbSet<TEntity> dbSet, IEnumerable<TEntity> entities) where TEntity : class, IEntity
    {
        if (_onSave is null) throw new Exception();
        if (!ReferenceEquals(dbSet.GetService<ICurrentDbContext>().Context, this)) throw new Exception();

        _onSave.Add(async () => await dbSet.SynchronizeAsync(entities));
        return Task.CompletedTask;
    }

    public Task BulkUpsertAsync<TEntity>(DbSet<TEntity> dbSet, IAsyncEnumerable<TEntity> entities) where TEntity : class, IEntity
    {
        if (_onSave is null) throw new Exception();
        if (!ReferenceEquals(dbSet.GetService<ICurrentDbContext>().Context, this)) throw new Exception();

        _onSave.Add(async () => await dbSet.UpsertAsync(entities));
        return Task.CompletedTask;
    }
    public Task BulkSynchronizeAsync<TEntity>(DbSet<TEntity> dbSet, IAsyncEnumerable<TEntity> entities) where TEntity : class, IEntity
    {
        if (_onSave is null) throw new Exception();
        if (!ReferenceEquals(dbSet.GetService<ICurrentDbContext>().Context, this)) throw new Exception();

        _onSave.Add(async () => await dbSet.SynchronizeAsync(entities));
        return Task.CompletedTask;
    }
}