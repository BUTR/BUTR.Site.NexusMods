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
        builder.Property<NexusModsModId>(nameof(NexusModsModEntity.NexusModsModId)).HasColumnName("nexusmods_mod_module_id").HasConversion<NexusModsModId.EfCoreValueConverter>().ValueGeneratedNever();
        builder.Property<ModuleId>(nameof(ModuleEntity.ModuleId)).HasColumnName("module_id").HasConversion<ModuleId.EfCoreValueConverter>();
        builder.Property(x => x.LinkType).HasColumnName("nexusmods_mod_module_link_type_id");
        builder.Property(x => x.LastUpdateDate).HasColumnName("date_of_last_update");
        builder.ToTable("nexusmods_mod_module", "nexusmods_mod").HasKey(nameof(NexusModsModToModuleEntity.TenantId), nameof(NexusModsModEntity.NexusModsModId), nameof(ModuleEntity.ModuleId), nameof(NexusModsModToModuleEntity.LinkType));

        builder.HasOne(x => x.NexusModsMod)
            .WithMany(x => x.ModuleIds)
            .HasForeignKey(nameof(NexusModsModToModuleEntity.TenantId), nameof(NexusModsModEntity.NexusModsModId))
            .HasPrincipalKey(x => new { x.TenantId, x.NexusModsModId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Module)
            .WithMany(x => x.ToNexusModsMods)
            .HasForeignKey(nameof(NexusModsModToModuleEntity.TenantId), nameof(ModuleEntity.ModuleId))
            .HasPrincipalKey(x => new { x.TenantId, x.ModuleId })
            .OnDelete(DeleteBehavior.Cascade);

        //builder.HasIndex(nameof(ModuleEntity.ModuleId));

        builder.Navigation(x => x.NexusModsMod).AutoInclude();
        builder.Navigation(x => x.Module).AutoInclude();

        base.ConfigureModel(builder);
    }
}