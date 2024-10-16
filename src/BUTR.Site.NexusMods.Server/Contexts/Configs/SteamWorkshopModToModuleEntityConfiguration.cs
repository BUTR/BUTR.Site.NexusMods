using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class SteamWorkshopModToModuleEntityConfiguration : BaseEntityConfigurationWithTenant<SteamWorkshopModToModuleEntity>
{
    public SteamWorkshopModToModuleEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<SteamWorkshopModToModuleEntity> builder)
    {
        builder.Property(x => x.SteamWorkshopModId).HasColumnName("steamworkshop_mod_module_id").HasVogenConversion().ValueGeneratedNever();
        builder.Property(x => x.ModuleId).HasColumnName("module_id").HasVogenConversion();
        builder.Property(x => x.LinkType).HasColumnName("mod_module_link_type_id");
        builder.Property(x => x.LastUpdateDate).HasColumnName("date_of_last_update");
        builder.ToTable("steamworkshop_mod_module", "steamworkshop_mod").HasKey(x => new
        {
            x.TenantId,
            x.SteamWorkshopModId,
            x.ModuleId,
            x.LinkType,
        });

        builder.HasOne(x => x.SteamWorkshopMod)
            .WithMany(x => x.ModuleIds)
            .HasForeignKey(x => new { x.TenantId, x.SteamWorkshopModId })
            .HasPrincipalKey(x => new { x.TenantId, x.SteamWorkshopModId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Module)
            .WithMany(x => x.ToSteamWorkshopMods)
            .HasForeignKey(x => new { x.TenantId, x.ModuleId })
            .HasPrincipalKey(x => new { x.TenantId, x.ModuleId })
            .OnDelete(DeleteBehavior.Cascade);

        //builder.HasIndex(nameof(ModuleEntity.ModuleId));

        base.ConfigureModel(builder);
    }
}