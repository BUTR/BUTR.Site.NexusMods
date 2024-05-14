using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Contexts;

public sealed class AppDbContextWrite : BaseAppDbContext
{
    private readonly ITenantContextAccessor _tenantContextAccessor;
    private readonly List<Func<AppDbContextWrite, Task>> _futureActions = new();

    private UpsertEntityFactory? _entityFactory;

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

    public UpsertEntityFactory GetEntityFactory() => _entityFactory ??= new UpsertEntityFactory(_tenantContextAccessor, this);

    public async Task SaveAsync(CancellationToken ct)
    {
        if (_entityFactory is not null)
            await _entityFactory.SaveCreatedAsync(ct);

        foreach (var futureAction in _futureActions)
            await futureAction(this);

        await SaveChangesAsync(cancellationToken: ct);

        _entityFactory = null;
        _futureActions.Clear();
    }

    public void AddFutureAction(Func<AppDbContextWrite, Task> action) => _futureActions.Add(action);
}