using BUTR.Site.NexusMods.Server.Options;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System;

namespace BUTR.Site.NexusMods.Server.Contexts;

public sealed class AppDbContextRead : BaseAppDbContext, IAppDbContextRead
{
    private readonly IDbContextFactory<AppDbContextRead> _dbContextFactory;

    public AppDbContextRead(
        IDbContextFactory<AppDbContextRead> dbContextFactory,
        IOptions<ConnectionStringsOptions> connectionStringOptions,
        DbContextOptions<AppDbContextRead> options,
        IEntityConfigurationFactory entityConfigurationFactory) : base(connectionStringOptions, options, entityConfigurationFactory)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        ChangeTracker.AutoDetectChangesEnabled = false;
        ChangeTracker.LazyLoadingEnabled = false;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
        }

        base.OnConfiguring(optionsBuilder);
    }

    public AppDbContextRead Create() => _dbContextFactory.CreateDbContext();
}