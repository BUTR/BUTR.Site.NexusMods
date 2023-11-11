using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class CrashReportToFileIdEntityConfiguration : BaseEntityConfigurationWithTenant<CrashReportToFileIdEntity>
{
    public CrashReportToFileIdEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<CrashReportToFileIdEntity> builder)
    {
        builder.Property(x => x.CrashReportId).HasColumnName("crash_report_file_id").HasVogenConversion().ValueGeneratedNever();
        builder.Property(x => x.FileId).HasColumnName("file_id").HasVogenConversion();
        builder.ToTable("crash_report_file", "crashreport").HasKey(x => new { x.TenantId, x.CrashReportId });

        builder.HasOne(x => x.ToCrashReport)
            .WithOne(x => x.FileId)
            .HasForeignKey<CrashReportToFileIdEntity>(x => new { x.TenantId, x.CrashReportId })
            .HasPrincipalKey<CrashReportEntity>(x => new { x.TenantId, x.CrashReportId })
            .OnDelete(DeleteBehavior.Cascade);

        //builder.HasIndex(x => x.FileId);

        base.ConfigureModel(builder);
    }
}