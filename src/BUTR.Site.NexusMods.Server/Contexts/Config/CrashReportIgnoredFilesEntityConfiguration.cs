using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Config
{
    public class CrashReportIgnoredFilesEntityConfiguration : BaseEntityConfiguration<CrashReportIgnoredFilesEntity>
    {
        protected override void ConfigureModel(EntityTypeBuilder<CrashReportIgnoredFilesEntity> builder)
        {
            builder.ToTable("crash_report_ignored_files").HasKey(p => p.Filename).HasName("crash_report_ignored_files_pkey");
            builder.Property(p => p.Filename).HasColumnName("filename").IsRequired();
        }
    }
}