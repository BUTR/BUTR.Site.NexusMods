using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class SteamWorkshopModToNameEntityConfiguration : BaseEntityConfigurationWithTenant<SteamWorkshopModToNameEntity>
{
    public SteamWorkshopModToNameEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<SteamWorkshopModToNameEntity> builder)
    {
        builder.Property(x => x.SteamWorkshopModId).HasColumnName("steamworkshop_mod_name_id").HasVogenConversion().ValueGeneratedNever();
        builder.Property(x => x.Name).HasColumnName("name");
        builder.ToTable("steamworkshop_mod_name", "steamworkshop_mod").HasKey(x => new
        {
            x.TenantId,
            x.SteamWorkshopModId,
        });

        builder.HasOne(x => x.SteamWorkshopMod)
            .WithOne(x => x.Name)
            .HasForeignKey<SteamWorkshopModToNameEntity>(x => new { x.TenantId, x.SteamWorkshopModId })
            .HasPrincipalKey<SteamWorkshopModEntity>(x => new { x.TenantId, x.SteamWorkshopModId })
            .OnDelete(DeleteBehavior.Cascade);

        base.ConfigureModel(builder);
    }
}