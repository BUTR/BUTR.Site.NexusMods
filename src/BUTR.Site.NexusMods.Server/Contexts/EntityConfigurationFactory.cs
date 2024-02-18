using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts.Configs;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;

namespace BUTR.Site.NexusMods.Server.Contexts;

[ScopedService<IEntityConfigurationFactory>]
public class EntityConfigurationFactory : IEntityConfigurationFactory
{
    private readonly IEntityConfiguration[] _entityConfigurations;

    public EntityConfigurationFactory(IEnumerable<IEntityConfiguration> entityConfigurations)
    {
        _entityConfigurations = entityConfigurations.ToArray();
    }

    public void ApplyConfiguration<TEntity>(ModelBuilder modelBuilder) where TEntity : class, IEntity
    {
        var configuration = _entityConfigurations.OfType<BaseEntityConfiguration<TEntity>>().First();
        configuration.Configure(modelBuilder);
    }

    public void ApplyConfigurationWithTenant<TEntity>(ModelBuilder modelBuilder) where TEntity : class, IEntityWithTenant
    {
        var configuration = _entityConfigurations.OfType<BaseEntityConfigurationWithTenant<TEntity>>().First();
        configuration.Configure(modelBuilder);
    }
}