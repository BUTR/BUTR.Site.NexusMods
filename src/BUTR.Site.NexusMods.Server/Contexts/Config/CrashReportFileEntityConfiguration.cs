using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System;

namespace BUTR.Site.NexusMods.Server.Contexts.Config;

public class CrashReportFileEntityConfiguration : BaseEntityConfiguration<CrashReportFileEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<CrashReportFileEntity> builder)
    {
        builder.ToTable("crash_report_file").HasKey(p => p.Filename).HasName("crash_report_file_entity_pkey");
        builder.Property(p => p.Filename).HasColumnName("filename").ValueGeneratedNever().IsRequired();
        builder.Property<Guid>("crash_report_id").IsRequired();
        builder.HasOne(p => p.CrashReport).WithOne().HasForeignKey<CrashReportFileEntity>("crash_report_id").HasConstraintName("FK_crash_report_file_entity_crash_report_id").OnDelete(DeleteBehavior.Cascade);
    }
}