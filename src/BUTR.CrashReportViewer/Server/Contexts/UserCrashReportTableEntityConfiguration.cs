using BUTR.CrashReportViewer.Server.Models.Contexts;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System;

namespace BUTR.CrashReportViewer.Server.Contexts
{
    public sealed class UserCrashReportTableEntityConfiguration : IEntityTypeConfiguration<UserCrashReportTable>
    {
        public void Configure(EntityTypeBuilder<UserCrashReportTable> builder)
        {
            builder.Property(p => p.UserId).HasColumnName("user_id").IsRequired();
            builder.Property(p => p.Status).HasColumnName("status").IsRequired();
            builder.Property(p => p.Comment).HasColumnName("comment").IsRequired();
            builder.Property<Guid>("CrashReportId").HasColumnName("crash_report_id").IsRequired();
            builder.ToTable("user_crash_report_entity").HasKey("UserId", "CrashReportId");
            builder.HasOne(p => p.CrashReport).WithMany(p => p.UserCrashReports).HasForeignKey("CrashReportId").OnDelete(DeleteBehavior.Cascade);
        }
    }
}