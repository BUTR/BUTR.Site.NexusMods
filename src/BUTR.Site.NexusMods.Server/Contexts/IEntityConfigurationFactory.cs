using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

namespace BUTR.Site.NexusMods.Server.Contexts;

public interface IEntityConfigurationFactory
{
    void ApplyConfiguration<TEntity>(ModelBuilder modelBuilder) where TEntity : class, IEntity;
    void ApplyConfigurationWithTenant<TEntity>(ModelBuilder modelBuilder) where TEntity : class, IEntityWithTenant;
}