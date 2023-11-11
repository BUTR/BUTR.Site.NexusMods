using BUTR.Site.NexusMods.Server.Options;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Contexts;

public sealed class AppDbContextRead : BaseAppDbContext, IAppDbContextRead
{
    public static Exception WriteNotSupported() => throw new NotSupportedException($"Write operations not supported with '{nameof(AppDbContextRead)}'!");


    private readonly IDbContextFactory<AppDbContextRead> _dbContextFactory;

    public AppDbContextRead(
        IDbContextFactory<AppDbContextRead> dbContextFactory,
        IOptions<ConnectionStringsOptions> connectionStringsOptions,
        IEntityConfigurationFactory entityConfigurationFactory,
        IOptions<JsonSerializerOptions> jsonSerializerOptions,
        DbContextOptions<AppDbContextRead> options) : base(connectionStringsOptions, entityConfigurationFactory, jsonSerializerOptions, options)
    {
        _dbContextFactory = dbContextFactory;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
        }

        base.OnConfiguring(optionsBuilder);
    }

    public AppDbContextRead New() => _dbContextFactory.CreateDbContext();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        throw WriteNotSupported();
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
    {
        throw WriteNotSupported();
    }
}