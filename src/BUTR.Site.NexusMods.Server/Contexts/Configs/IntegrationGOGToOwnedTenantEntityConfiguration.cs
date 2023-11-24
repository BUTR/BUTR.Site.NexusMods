using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class IntegrationGOGToOwnedTenantEntityConfiguration : BaseEntityConfiguration<IntegrationGOGToOwnedTenantEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<IntegrationGOGToOwnedTenantEntity> builder)
    {
        builder.Property(x => x.GOGUserId).HasColumnName("integration_gog_owned_tenant_id");
        builder.Property(x => x.OwnedTenant).HasColumnName("owned_tenant").HasValueObjectConversion();
        builder.ToTable("integration_gog_owned_tenant", "integration").HasKey(x => new { x.GOGUserId, x.OwnedTenant });

        builder.HasOne<NexusModsUserToIntegrationGOGEntity>()
            .WithMany()
            .HasForeignKey(x => x.GOGUserId)
            .HasPrincipalKey(x => x.GOGUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<TenantEntity>()
            .WithMany()
            .HasForeignKey(x => x.OwnedTenant)
            .HasPrincipalKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        base.ConfigureModel(builder);
    }
}