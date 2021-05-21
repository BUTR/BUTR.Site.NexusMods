using BUTR.CrashReportViewer.Server.Models.Contexts;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using System;
using System.Linq;

namespace BUTR.CrashReportViewer.Server.Contexts
{
    public sealed class CrashReportTableEntityConfiguration : IEntityTypeConfiguration<CrashReportTable>
    {
        public void Configure(EntityTypeBuilder<CrashReportTable> builder)
        {
            var converter = new ValueConverter<int[], string>(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(int.Parse).ToArray());
            var valueComparer = new ValueComparer<int[]>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToArray());

            builder.ToTable("crash_report_entity").HasKey(p => p.Id);
            builder.Property(p => p.Id).HasColumnName("id");
            builder.Property(p => p.Exception).HasColumnName("exception").IsRequired();
            builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(p => p.ModIds).HasColumnName("mod_ids").HasConversion(converter).IsRequired();
            builder.Property(p => p.ModIds).Metadata.SetValueComparer(valueComparer);
        }
    }
}