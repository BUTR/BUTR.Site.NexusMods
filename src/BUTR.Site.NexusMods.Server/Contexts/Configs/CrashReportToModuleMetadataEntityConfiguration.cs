using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class CrashReportToModuleMetadataEntityConfiguration : BaseEntityConfigurationWithTenant<CrashReportToModuleMetadataEntity>
{
    public CrashReportToModuleMetadataEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<CrashReportToModuleMetadataEntity> builder)
    {
        builder.Property(x => x.CrashReportId).HasColumnName("crash_report_module_info_id").HasValueObjectConversion().ValueGeneratedNever();
        builder.Property<ModuleId>(nameof(ModuleEntity.ModuleId)).HasColumnName("module_id").HasValueObjectConversion();
        builder.Property(x => x.Version).HasValueObjectConversion().HasColumnName("version");
        builder.Property<NexusModsModId?>(nameof(NexusModsModEntity.NexusModsModId)).HasColumnName("nexusmods_mod_id").HasValueObjectConversion().IsRequired(false);
        builder.Property(x => x.Index).HasColumnName("index");
        builder.Property(x => x.IsInvolved).HasColumnName("is_involved");
        builder.ToTable("crash_report_module_info", "crashreport").HasKey(nameof(CrashReportToModuleMetadataEntity.TenantId), nameof(CrashReportToModuleMetadataEntity.CrashReportId), nameof(ModuleEntity.ModuleId));

        builder.HasOne(x => x.ToCrashReport)
            .WithMany(x => x.ModuleInfos)
            .HasForeignKey(x => new { x.TenantId, x.CrashReportId })
            .HasPrincipalKey(x => new { x.TenantId, x.CrashReportId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.NexusModsMod)
            .WithMany()
            .HasForeignKey(nameof(NexusModsModEntity.TenantId), nameof(NexusModsModEntity.NexusModsModId))
            .HasPrincipalKey(x => new { x.TenantId, x.NexusModsModId })
            .OnDelete(DeleteBehavior.SetNull);

        //builder.HasIndex(x => x.CrashReportId);
        //builder.HasIndex(nameof(ModuleEntity.ModuleId));
        //builder.HasIndex(nameof(NexusModsModEntity.ModId));

        builder.Navigation(x => x.Module).AutoInclude();
        builder.Navigation(x => x.NexusModsMod).AutoInclude();

        base.ConfigureModel(builder);
    }
}