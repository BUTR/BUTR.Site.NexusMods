using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Options;
using BUTR.Site.NexusMods.Server.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Z.BulkOperations;

namespace BUTR.Site.NexusMods.Server.Contexts;

public sealed class AppDbContextWrite : BaseAppDbContext, IAppDbContextWrite
{
    private readonly IDbContextFactory<AppDbContextWrite> _dbContextFactory;
    private readonly ITenantContextAccessor _tenantContextAccessor;
    private EntityFactory? _entityFactory;

    public AppDbContextWrite(
        IDbContextFactory<AppDbContextWrite> dbContextFactory,
        IOptions<ConnectionStringsOptions> connectionStringOptions,
        DbContextOptions<AppDbContextWrite> options,
        ITenantContextAccessor tenantContextAccessor,
        IEntityConfigurationFactory entityConfigurationFactory) : base(connectionStringOptions, options, entityConfigurationFactory)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _tenantContextAccessor = tenantContextAccessor ?? throw new ArgumentNullException(nameof(tenantContextAccessor));
        ChangeTracker.LazyLoadingEnabled = false;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
        }

        base.OnConfiguring(optionsBuilder);
    }

    public AppDbContextWrite Create() => _dbContextFactory.CreateDbContext();

    public EntityFactory CreateEntityFactory() => _entityFactory ??= new EntityFactory(_tenantContextAccessor, this);

    public IAsyncDisposable CreateSaveScope() => new AppContextSaveScope(this, CreateEntityFactory());

    public void FutureUpsert<TEntity>(Func<IAppDbContextWrite, DbSet<TEntity>> dbset, TEntity entity) where TEntity : class, IEntity
    {
        FutureAction(x => dbset(x).SingleMerge(entity));
    }

    public void FutureUpsert<TEntity>(Func<IAppDbContextWrite, DbSet<TEntity>> dbset, ICollection<TEntity> entities) where TEntity : class, IEntity
    {
        FutureAction(x => dbset(x).BulkMerge(entities));
    }

    public void FutureSyncronize<TEntity>(Func<IAppDbContextWrite, DbSet<TEntity>> dbset, ICollection<TEntity> entities) where TEntity : class, IEntity
    {
        FutureAction(x => dbset(x).BulkSynchronize(entities));
    }

    public void FutureAction(Action<IAppDbContextWrite> action) => this.FutureAction<AppDbContextWrite>(action);

    public async Task SaveAsync(CancellationToken ct)
    {
        if (IsReadOnly)
        {
            throw new NotSupportedException("The Save method is not supported on read-only database contexts.");
        }

        try
        {
            await Database.BeginTransactionAsync(ct);
            if (_entityFactory is not null)
                await _entityFactory.SaveCreated(ct);
            this.ExecuteFutureAction(false);
            await this.BulkSaveChangesAsync(ct);
            await Database.CommitTransactionAsync(ct);
        }
        catch (Exception e)
        {
            await Database.RollbackTransactionAsync(ct);
            throw;
        }
    }

    public override async ValueTask DisposeAsync()
    {
        await SaveAsync(CancellationToken.None);
        await base.DisposeAsync();
    }
}