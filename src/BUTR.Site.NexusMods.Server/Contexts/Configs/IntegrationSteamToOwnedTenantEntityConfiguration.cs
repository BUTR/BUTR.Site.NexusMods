using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class IntegrationSteamToOwnedTenantEntityConfiguration : BaseEntityConfiguration<IntegrationSteamToOwnedTenantEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<IntegrationSteamToOwnedTenantEntity> builder)
    {
        builder.Property(x => x.SteamUserId).HasColumnName("integration_steam_owned_tenant_id");
        builder.Property(x => x.OwnedTenant).HasColumnName("owned_tenant").HasVogenConversion();
        builder.ToTable("integration_steam_owned_tenant", "integration").HasKey(x => new { x.SteamUserId, x.OwnedTenant });

        builder.HasOne<NexusModsUserToIntegrationSteamEntity>()
            .WithMany()
            .HasForeignKey(x => x.SteamUserId)
            .HasPrincipalKey(x => x.SteamUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<TenantEntity>()
            .WithMany()
            .HasForeignKey(x => x.OwnedTenant)
            .HasPrincipalKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        base.ConfigureModel(builder);
    }
}