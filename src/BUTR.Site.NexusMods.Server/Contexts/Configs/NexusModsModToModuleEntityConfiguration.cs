using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class NexusModsModToModuleEntityConfiguration : BaseEntityConfigurationWithTenant<NexusModsModToModuleEntity>
{
    public NexusModsModToModuleEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<NexusModsModToModuleEntity> builder)
    {
        builder.Property(x => x.NexusModsModId).HasColumnName("nexusmods_mod_module_id").HasVogenConversion().ValueGeneratedNever();
        builder.Property(x => x.ModuleId).HasColumnName("module_id").HasVogenConversion();
        builder.Property(x => x.LinkType).HasColumnName("nexusmods_mod_module_link_type_id");
        builder.Property(x => x.LastUpdateDate).HasColumnName("date_of_last_update");
        builder.ToTable("nexusmods_mod_module", "nexusmods_mod").HasKey(x => new
        {
            x.TenantId,
            x.NexusModsModId,
            x.ModuleId,
            x.LinkType,
        });

        builder.HasOne(x => x.NexusModsMod)
            .WithMany(x => x.ModuleIds)
            .HasForeignKey(x => new { x.TenantId, x.NexusModsModId })
            .HasPrincipalKey(x => new { x.TenantId, x.NexusModsModId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Module)
            .WithMany(x => x.ToNexusModsMods)
            .HasForeignKey(x => new { x.TenantId, x.ModuleId })
            .HasPrincipalKey(x => new { x.TenantId, x.ModuleId })
            .OnDelete(DeleteBehavior.Cascade);

        //builder.HasIndex(nameof(ModuleEntity.ModuleId));

        base.ConfigureModel(builder);
    }
}