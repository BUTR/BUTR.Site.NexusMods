using BUTR.Site.NexusMods.Server.Contexts.Configs;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using System;

namespace BUTR.Site.NexusMods.Server.Contexts;

public class EntityConfigurationFactory : IEntityConfigurationFactory
{
    private readonly IServiceProvider _serviceProvider;

    public EntityConfigurationFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void ApplyConfiguration<TEntity>(ModelBuilder modelBuilder) where TEntity : class, IEntity
    {
        var configuration = _serviceProvider.GetRequiredService<BaseEntityConfiguration<TEntity>>();
        configuration.Configure(modelBuilder.Entity<TEntity>());
    }

    public void ApplyConfigurationWithTenant<TEntity>(ModelBuilder modelBuilder) where TEntity : class, IEntityWithTenant
    {
        var configuration = _serviceProvider.GetRequiredService<BaseEntityConfigurationWithTenant<TEntity>>();
        configuration.Configure(modelBuilder.Entity<TEntity>());
    }
}