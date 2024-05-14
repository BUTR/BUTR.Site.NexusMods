using Microsoft.EntityFrameworkCore;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Contexts;

public sealed class AppDbContextRead : BaseAppDbContext
{
    public override bool IsReadOnly => true;

    public static Exception WriteNotSupported() => throw new NotSupportedException($"Write operations not supported with '{nameof(AppDbContextRead)}'!");

    public AppDbContextRead(
        NpgsqlDataSourceProvider dataSourceProvider,
        IEntityConfigurationFactory entityConfigurationFactory,
        DbContextOptions<AppDbContextRead> options) : base(dataSourceProvider, entityConfigurationFactory, options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);

        base.OnConfiguring(optionsBuilder);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess) => throw WriteNotSupported();
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken ct = default) => throw WriteNotSupported();
}