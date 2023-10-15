using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Options;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

    public EntityFactory GetEntityFactory() => _entityFactory ??= new EntityFactory(_tenantContextAccessor, this);

    public IAsyncDisposable CreateSaveScope() => new AppContextSaveScope(this);

    public void FutureUpsert<TEntity>(Func<IAppDbContextWrite, DbSet<TEntity>> dbset, TEntity entity) where TEntity : class, IEntity =>
        FutureAction(x => dbset(x).SingleMerge(entity));

    public void FutureUpsert<TEntity>(Func<IAppDbContextWrite, DbSet<TEntity>> dbset, ICollection<TEntity> entities) where TEntity : class, IEntity =>
        FutureAction(x => dbset(x).BulkMerge(entities));

    public void FutureSyncronize<TEntity>(Func<IAppDbContextWrite, DbSet<TEntity>> dbset, ICollection<TEntity> entities) where TEntity : class, IEntity =>
        FutureAction(x => dbset(x).BulkSynchronize(entities));

    private void FutureAction(Action<IAppDbContextWrite> action) => this.FutureAction<AppDbContextWrite>(action);

    public async Task SaveAsync(CancellationToken ct)
    {
        if (IsReadOnly)
            throw new NotSupportedException("The Save method is not supported on read-only database contexts.");

        await using var transaction = await Database.BeginTransactionAsync(ct);
        try
        {
            if (_entityFactory is not null) await _entityFactory.SaveCreatedAsync(ct);
            this.ExecuteFutureAction(false);
            await this.BulkSaveChangesAsync(o => o.UseInternalTransaction = false, ct);
            await transaction.CommitAsync(ct);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public override async ValueTask DisposeAsync()
    {
        await SaveAsync(CancellationToken.None);
        await base.DisposeAsync();
    }
}