using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class, IEntity
{
    public void Configure(EntityTypeBuilder<TEntity> builder) => ConfigureModel(builder);
    protected virtual void ConfigureModel(EntityTypeBuilder<TEntity> builder) { }
}