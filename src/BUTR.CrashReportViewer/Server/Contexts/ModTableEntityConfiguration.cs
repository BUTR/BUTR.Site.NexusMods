using BUTR.CrashReportViewer.Server.Models.Contexts;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.CrashReportViewer.Server.Contexts
{
    public sealed class ModTableEntityConfiguration : IEntityTypeConfiguration<ModTable>
    {
        public void Configure(EntityTypeBuilder<ModTable> builder)
        {
            builder.ToTable("mod_entity").HasKey(p => new { p.GameDomain, p.ModId });
            builder.Property(p => p.GameDomain).HasColumnName("game_domain").IsRequired();
            builder.Property(p => p.ModId).HasColumnName("mod_id").IsRequired();
            builder.Property(p => p.Name).HasColumnName("name").IsRequired();
            builder.Property(p => p.UserIds).HasColumnName("user_ids").IsRequired();
        }
    }
}