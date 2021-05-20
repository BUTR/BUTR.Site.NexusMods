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
            builder.ToTable("user_crash_report_entity").HasKey(p => p.Id);
            builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(p => p.UserId).HasColumnName("user_id").IsRequired();
            builder.Property(p => p.Status).HasColumnName("status").IsRequired();
            builder.Property(p => p.Comment).HasColumnName("comment").IsRequired();
            builder.Property<Guid>("CrashReportForeignKey").HasColumnName("crash_report_fk").IsRequired();
            builder.HasOne(p => p.CrashReport).WithMany(p => p.UserCrashReports).HasForeignKey("CrashReportForeignKey").OnDelete(DeleteBehavior.Cascade);
        }
    }
}