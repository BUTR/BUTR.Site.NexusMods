using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public abstract class BaseEntityConfigurationWithTenant<TEntity> : BaseEntityConfiguration<TEntity> where TEntity : class, IEntityWithTenant
{
    private readonly ITenantContextAccessor _tenantContextAccessor;

    protected BaseEntityConfigurationWithTenant(ITenantContextAccessor tenantContextAccessor)
    {
        _tenantContextAccessor = tenantContextAccessor;
    }

    protected override void ConfigureModel(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(x => x.TenantId).HasColumnName("tenant").HasVogenConversion();
        builder.HasQueryFilter(x => x.TenantId.Equals(_tenantContextAccessor.Current));

        builder.HasOne<TenantEntity>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasPrincipalKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}