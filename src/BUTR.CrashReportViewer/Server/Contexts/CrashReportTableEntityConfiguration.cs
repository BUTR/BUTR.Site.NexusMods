using BUTR.CrashReportViewer.Server.Models.Contexts;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.CrashReportViewer.Server.Contexts
{
    public sealed class CrashReportTableEntityConfiguration : IEntityTypeConfiguration<CrashReportTable>
    {
        public void Configure(EntityTypeBuilder<CrashReportTable> builder)
        {
            builder.ToTable("crash_report_entity").HasKey(p => p.Id);
            builder.Property(p => p.Id).HasColumnName("id");
            builder.Property(p => p.Exception).HasColumnName("exception").IsRequired();
            builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(p => p.ModIds).HasColumnName("mod_ids").IsRequired();
            builder.Property(p => p.Url).HasColumnName("url").IsRequired();
        }
    }
}