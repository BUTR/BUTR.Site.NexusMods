using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class NexusModsUserToCrashReportEntityConfiguration : BaseEntityConfigurationWithTenant<NexusModsUserToCrashReportEntity>
{
    public NexusModsUserToCrashReportEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<NexusModsUserToCrashReportEntity> builder)
    {
        builder.Property(x => x.NexusModsUserId).HasColumnName("nexusmods_user_crash_report_id").HasVogenConversion().ValueGeneratedNever();
        builder.Property(x => x.CrashReportId).HasColumnName("crash_report_id").HasVogenConversion();
        builder.Property(x => x.Status).HasColumnName("status");
        builder.Property(x => x.Comment).HasColumnName("comment");
        builder.ToTable("nexusmods_user_crash_report", "nexusmods_user").HasKey(x => new
        {
            x.TenantId,
            x.NexusModsUserId,
            x.CrashReportId,
        });

        builder.HasOne(x => x.NexusModsUser)
            .WithMany(x => x.ToCrashReports)
            .HasForeignKey(x => x.NexusModsUserId)
            .HasPrincipalKey(x => x.NexusModsUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ToCrashReport)
            .WithMany(x => x.ToUsers)
            .HasForeignKey(x => new { x.TenantId, x.CrashReportId })
            .HasPrincipalKey(x => new { x.TenantId, x.CrashReportId })
            .OnDelete(DeleteBehavior.Cascade);

        base.ConfigureModel(builder);
    }
}