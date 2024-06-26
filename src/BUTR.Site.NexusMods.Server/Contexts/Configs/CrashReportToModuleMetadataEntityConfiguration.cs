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
        builder.Property(x => x.CrashReportId).HasColumnName("crash_report_module_info_id").HasVogenConversion().ValueGeneratedNever();
        builder.Property(x => x.ModuleId).HasColumnName("module_id").HasVogenConversion();
        builder.Property(x => x.Version).HasVogenConversion().HasColumnName("version");
        builder.Property(x => x.NexusModsModId).HasColumnName("nexusmods_mod_id").HasConversion<NexusModsModId.EfCoreValueConverter, NexusModsModId.EfCoreValueComparer>().IsRequired(false);
        builder.Property(x => x.InvolvedPosition).HasColumnName("involved_position");
        builder.Property(x => x.IsInvolved).HasColumnName("is_involved");
        builder.ToTable("crash_report_module_info", "crashreport").HasKey(x => new
        {
            x.TenantId,
            x.CrashReportId,
            x.ModuleId
        });

        builder.HasOne(x => x.ToCrashReport)
            .WithMany(x => x.ModuleInfos)
            .HasForeignKey(x => new { x.TenantId, x.CrashReportId })
            .HasPrincipalKey(x => new { x.TenantId, x.CrashReportId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.NexusModsMod)
            .WithMany()
            .HasForeignKey(x => new { x.TenantId, x.NexusModsModId })
            .HasPrincipalKey(x => new { x.TenantId, x.NexusModsModId })
            .OnDelete(DeleteBehavior.SetNull);

        //builder.HasIndex(x => x.CrashReportId);
        //builder.HasIndex(nameof(ModuleEntity.ModuleId));
        //builder.HasIndex(nameof(NexusModsModEntity.ModId));

        base.ConfigureModel(builder);
    }
}