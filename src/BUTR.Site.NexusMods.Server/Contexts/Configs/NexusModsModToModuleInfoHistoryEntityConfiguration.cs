using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class NexusModsModToModuleInfoHistoryEntityConfiguration : BaseEntityConfigurationWithTenant<NexusModsModToModuleInfoHistoryEntity>
{
    public NexusModsModToModuleInfoHistoryEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<NexusModsModToModuleInfoHistoryEntity> builder)
    {
        builder.Property<NexusModsModId>(nameof(NexusModsModEntity.NexusModsModId)).HasColumnName("nexusmods_mod_name_id").HasConversion<NexusModsModId.EfCoreValueConverter>().ValueGeneratedNever();
        builder.Property<ModuleId>(nameof(ModuleEntity.ModuleId)).HasColumnName("module_id").HasConversion<ModuleId.EfCoreValueConverter>();
        builder.Property(x => x.ModuleVersion).HasColumnName("module_version").HasConversion<ModuleVersion.EfCoreValueConverter>();
        builder.Property(x => x.ModuleInfo).HasColumnName("module_info").HasColumnType("jsonb");
        builder.ToTable("nexusmods_mod_module_info_history", "nexusmods_mod").HasKey(nameof(NexusModsModToModuleInfoHistoryEntity.TenantId), nameof(NexusModsModEntity.NexusModsModId), nameof(ModuleEntity.ModuleId), nameof(NexusModsModToModuleInfoHistoryEntity.ModuleVersion));

        builder.HasOne(x => x.NexusModsMod)
            .WithMany()
            .HasForeignKey(nameof(NexusModsModToModuleInfoHistoryEntity.TenantId), nameof(NexusModsModEntity.NexusModsModId))
            .HasPrincipalKey(x => new { x.TenantId, x.NexusModsModId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(nameof(NexusModsModToModuleInfoHistoryEntity.TenantId), nameof(ModuleEntity.ModuleId))
            .HasPrincipalKey(x => new { x.TenantId, x.ModuleId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.NexusModsMod).AutoInclude();
        builder.Navigation(x => x.Module).AutoInclude();

        base.ConfigureModel(builder);
    }
}