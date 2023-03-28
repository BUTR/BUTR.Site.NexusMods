using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Config
{
    public class CrashScoreInvolvedEntityConfiguration : BaseEntityConfiguration<CrashScoreInvolvedEntity>
    {
        protected override void ConfigureModel(EntityTypeBuilder<CrashScoreInvolvedEntity> builder)
        {
            builder.ToTable("crash_score_involved_entity").HasNoKey();
            builder.Property(p => p.GameVersion).HasColumnName("game_version").IsRequired();
            builder.Property(p => p.ModId).HasColumnName("mod_id").IsRequired();
            builder.Property(p => p.ModVersion).HasColumnName("version").IsRequired();
            builder.Property(p => p.InvolvedCount).HasColumnName("involved_count").IsRequired();
            builder.Property(p => p.NotInvolvedCount).HasColumnName("not_involved_count").IsRequired();
            builder.Property(p => p.TotalCount).HasColumnName("total_count").IsRequired();
            builder.Property(p => p.RawValue).HasColumnName("value").IsRequired();
            builder.Property(p => p.Score).HasColumnName("crash_score").IsRequired();
        }
    }
}