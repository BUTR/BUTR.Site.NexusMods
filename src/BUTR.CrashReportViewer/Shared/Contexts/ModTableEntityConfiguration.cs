using BUTR.CrashReportViewer.Shared.Models.Contexts;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using System;
using System.Linq;

namespace BUTR.CrashReportViewer.Shared.Contexts
{
    public sealed class ModTableEntityConfiguration : IEntityTypeConfiguration<ModTable>
    {
        public void Configure(EntityTypeBuilder<ModTable> builder)
        {
            var converter = new ValueConverter<int[], string>(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(int.Parse).ToArray());
            var valueComparer = new ValueComparer<int[]>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToArray());

            builder.ToTable("mod_entity").HasKey(p => new { p.GameDomain, p.ModId });
            builder.Property(p => p.GameDomain).HasColumnName("game_domain").IsRequired();
            builder.Property(p => p.ModId).HasColumnName("mod_id").IsRequired();
            builder.Property(p => p.Name).HasColumnName("name").IsRequired();
            builder.Property(p => p.UserIds).HasColumnName("user_ids").HasConversion(converter).IsRequired().Metadata.SetValueComparer(valueComparer);
        }
    }
}