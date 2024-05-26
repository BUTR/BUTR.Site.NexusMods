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
        builder.Property(x => x.NexusModsModId).HasColumnName("nexusmods_mod_module_info_history_id").HasVogenConversion().ValueGeneratedNever();
        builder.Property(x => x.NexusModsFileId).HasColumnName("nexusmods_file_id").HasVogenConversion();
        builder.Property(x => x.ModuleId).HasColumnName("module_id").HasVogenConversion();
        builder.Property(x => x.ModuleVersion).HasColumnName("module_version").HasVogenConversion();
        builder.Property(x => x.ModuleInfo).HasColumnName("module_info").HasColumnType("jsonb");
        builder.Property(x => x.UploadDate).HasColumnName("date_of_upload");
        builder.ToTable("nexusmods_mod_module_info_history", "nexusmods_mod").HasKey(x => new
        {
            x.TenantId,
            x.NexusModsFileId,
            x.NexusModsModId,
            x.ModuleId,
            x.ModuleVersion,
        });

        builder.HasOne(x => x.NexusModsMod)
            .WithMany()
            .HasForeignKey(x => new { x.TenantId, x.NexusModsModId })
            .HasPrincipalKey(x => new { x.TenantId, x.NexusModsModId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => new { x.TenantId, x.ModuleId })
            .HasPrincipalKey(x => new { x.TenantId, x.ModuleId })
            .OnDelete(DeleteBehavior.Cascade);

        base.ConfigureModel(builder);
    }
}