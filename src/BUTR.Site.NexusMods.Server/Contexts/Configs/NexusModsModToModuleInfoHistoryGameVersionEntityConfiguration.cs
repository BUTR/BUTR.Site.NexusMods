using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class NexusModsModToModuleInfoHistoryGameVersionEntityConfiguration : BaseEntityConfigurationWithTenant<NexusModsModToModuleInfoHistoryGameVersionEntity>
{
    public NexusModsModToModuleInfoHistoryGameVersionEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<NexusModsModToModuleInfoHistoryGameVersionEntity> builder)
    {
        var primaryKeys = new []
        {
            nameof(NexusModsModToModuleInfoHistoryGameVersionEntity.TenantId),
            nameof(NexusModsModToModuleInfoHistoryGameVersionEntity.NexusModsFileId),
            nameof(NexusModsModEntity.NexusModsModId),
            nameof(ModuleEntity.ModuleId),
            nameof(NexusModsModToModuleInfoHistoryGameVersionEntity.ModuleVersion)
        };
        
        builder.Property<NexusModsModId>(nameof(NexusModsModEntity.NexusModsModId)).HasColumnName("nexusmods_mod_module_info_history_game_version_id").HasValueObjectConversion().ValueGeneratedNever();
        builder.Property(x => x.NexusModsFileId).HasColumnName("nexusmods_file_id").HasValueObjectConversion();
        builder.Property<ModuleId>(nameof(ModuleEntity.ModuleId)).HasColumnName("module_id").HasValueObjectConversion();
        builder.Property(x => x.ModuleVersion).HasColumnName("module_version").HasValueObjectConversion();
        builder.Property(x => x.GameVersion).HasColumnName("game_version").HasValueObjectConversion();
        builder.ToTable("nexusmods_mod_module_info_history_game_version", "nexusmods_mod").HasKey(primaryKeys);

        builder.HasOne(x => x.NexusModsMod)
            .WithMany()
            .HasForeignKey(nameof(NexusModsModToModuleInfoHistoryGameVersionEntity.TenantId), nameof(NexusModsModEntity.NexusModsModId))
            .HasPrincipalKey(x => new { x.TenantId, x.NexusModsModId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(nameof(NexusModsModToModuleInfoHistoryGameVersionEntity.TenantId), nameof(ModuleEntity.ModuleId))
            .HasPrincipalKey(x => new { x.TenantId, x.ModuleId })
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(x => x.MainEntity)
            .WithMany(x => x.GameVersions)
            .HasForeignKey(primaryKeys)
            .HasPrincipalKey(primaryKeys)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.NexusModsMod).AutoInclude();
        builder.Navigation(x => x.Module).AutoInclude();

        base.ConfigureModel(builder);
    }
}