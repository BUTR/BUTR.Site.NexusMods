using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class CrashReportEntityConfiguration : BaseEntityConfigurationWithTenant<CrashReportEntity>
{
    public CrashReportEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<CrashReportEntity> builder)
    {
        builder.Property(x => x.CrashReportId).HasColumnName("crash_report_id").HasValueObjectConversion().ValueGeneratedNever();
        builder.Property(x => x.Version).HasColumnName("version").HasValueObjectConversion();
        builder.Property(x => x.GameVersion).HasColumnName("game_version").HasValueObjectConversion();
        builder.Property<ExceptionTypeId>(nameof(ExceptionTypeEntity.ExceptionTypeId)).HasColumnName("exception_type_id").HasValueObjectConversion();
        builder.Property(x => x.Exception).HasColumnName("exception");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.Url).HasColumnName("url").HasValueObjectConversion();
        builder.ToTable("crash_report", "crashreport").HasKey(x => new { x.TenantId, x.CrashReportId });

        builder.HasOne(x => x.ExceptionType)
            .WithMany(x => x.ToCrashReports)
            .HasForeignKey(nameof(CrashReportEntity.TenantId), nameof(ExceptionTypeEntity.ExceptionTypeId))
            .HasPrincipalKey(x => new { x.TenantId, x.ExceptionTypeId })
            .OnDelete(DeleteBehavior.Cascade);

        //builder.HasIndex(x => x.CrashReportId).IsUnique();
        //builder.HasIndex(x => x.Version);
        //builder.HasIndex(x => x.GameVersion);
        //builder.HasIndex(x => x.CreatedAt);

        builder.Navigation(x => x.ExceptionType).AutoInclude();

        base.ConfigureModel(builder);
    }
}