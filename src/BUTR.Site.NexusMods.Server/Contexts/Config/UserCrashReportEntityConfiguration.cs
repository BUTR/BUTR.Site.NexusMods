using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System;

namespace BUTR.Site.NexusMods.Server.Contexts.Config
{
    public class UserCrashReportEntityConfiguration : BaseEntityConfiguration<UserCrashReportEntity>
    {
        protected override void ConfigureModel(EntityTypeBuilder<UserCrashReportEntity> builder)
        {
            builder.ToTable("user_crash_report_entity").HasKey(p => p.UserId).HasName("user_crash_report_entity_pkey");
            builder.Property(p => p.UserId).HasColumnName("user_id").ValueGeneratedNever().IsRequired();
            builder.Property(p => p.Status).HasColumnName("status").IsRequired();
            builder.Property(p => p.Comment).HasColumnName("comment").IsRequired();
            builder.Property<Guid>("crash_report_id").IsRequired();
            builder.HasOne(p => p.CrashReport).WithMany().HasForeignKey("crash_report_id").HasConstraintName("FK_user_crash_report_entity_crash_report_entity_crash_report_id").OnDelete(DeleteBehavior.Cascade);
        }
    }
}