using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class CrashReportIgnoredFileIdEntityConfiguration : BaseEntityConfigurationWithTenant<CrashReportIgnoredFileEntity>
{
    public CrashReportIgnoredFileIdEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<CrashReportIgnoredFileEntity> builder)
    {
        builder.Property(x => x.CrashReportFileId).HasColumnName("crash_report_file_ignored_id").HasVogenConversion().ValueGeneratedOnAdd();
        builder.ToTable("crash_report_file_ignored", "crashreport").HasKey(x => new { x.TenantId, Value = x.CrashReportFileId });

        base.ConfigureModel(builder);
    }
}