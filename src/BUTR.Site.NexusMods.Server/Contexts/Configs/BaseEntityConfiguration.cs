using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public abstract class BaseEntityConfiguration<TEntity> where TEntity : class, IEntity
{
    public virtual void Configure(ModelBuilder builder) => ConfigureModel(builder.Entity<TEntity>());
    protected virtual void ConfigureModel(EntityTypeBuilder<TEntity> builder) { }
}