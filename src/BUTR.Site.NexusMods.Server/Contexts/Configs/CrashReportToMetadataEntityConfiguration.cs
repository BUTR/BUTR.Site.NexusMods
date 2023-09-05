using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class CrashReportToMetadataEntityConfiguration : BaseEntityConfigurationWithTenant<CrashReportToMetadataEntity>
{
    public CrashReportToMetadataEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<CrashReportToMetadataEntity> builder)
    {
        builder.Property(x => x.CrashReportId).HasColumnName("crash_report_metadata_id").HasConversion<CrashReportId.EfCoreValueConverter>().ValueGeneratedNever();
        builder.Property(x => x.LauncherType).HasColumnName("launcher_type");
        builder.Property(x => x.LauncherVersion).HasColumnName("launcher_version");
        builder.Property(x => x.Runtime).HasColumnName("runtime");
        builder.Property(x => x.BUTRLoaderVersion).HasColumnName("butrloader_version");
        builder.Property(x => x.BLSEVersion).HasColumnName("blse_version");
        builder.Property(x => x.LauncherExVersion).HasColumnName("launcherex_version");
        builder.ToTable("crash_report_metadata", "crashreport").HasKey(x => new { x.TenantId, x.CrashReportId });

        builder.HasOne(x => x.ToCrashReport)
            .WithOne(x => x.Metadata)
            .HasForeignKey<CrashReportToMetadataEntity>(x => new { x.TenantId, x.CrashReportId })
            .HasPrincipalKey<CrashReportEntity>(x => new { x.TenantId, x.CrashReportId })
            .OnDelete(DeleteBehavior.Cascade);

        base.ConfigureModel(builder);
    }
}