using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class TenantEntityConfiguration : BaseEntityConfiguration<TenantEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<TenantEntity> builder)
    {
        builder.Property(x => x.TenantId).HasColumnName("tenant_id").HasValueObjectConversion();
        builder.ToTable("tenant", "tenant").HasKey(x => x.TenantId);

        base.ConfigureModel(builder);
    }
}