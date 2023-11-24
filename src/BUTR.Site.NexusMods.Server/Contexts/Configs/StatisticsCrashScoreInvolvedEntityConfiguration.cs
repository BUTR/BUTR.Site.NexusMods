using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class StatisticsCrashScoreInvolvedEntityConfiguration : BaseEntityConfigurationWithTenant<StatisticsCrashScoreInvolvedEntity>
{
    public StatisticsCrashScoreInvolvedEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<StatisticsCrashScoreInvolvedEntity> builder)
    {
        builder.Property(x => x.StatisticsCrashScoreInvolvedId).HasColumnName("crash_score_involved_id");
        builder.Property(x => x.GameVersion).HasColumnName("game_version").HasValueObjectConversion();
        builder.Property<ModuleId>(nameof(ModuleEntity.ModuleId)).HasColumnName("module_id").HasValueObjectConversion();
        builder.Property(x => x.ModuleVersion).HasColumnName("module_version").HasValueObjectConversion();
        builder.Property(x => x.InvolvedCount).HasColumnName("involved_count");
        builder.Property(x => x.NotInvolvedCount).HasColumnName("not_involved_count");
        builder.Property(x => x.TotalCount).HasColumnName("total_count");
        builder.Property(x => x.RawValue).HasColumnName("value");
        builder.Property(x => x.Score).HasColumnName("crash_score");
        builder.ToTable("crash_score_involved", "statistics").HasKey(x => new { x.TenantId, x.StatisticsCrashScoreInvolvedId });

        builder.HasOne(x => x.Module)
            .WithMany(x => x.ToCrashScore)
            .HasForeignKey(nameof(StatisticsCrashScoreInvolvedEntity.TenantId), nameof(ModuleEntity.ModuleId))
            .HasPrincipalKey(x => new { x.TenantId, x.ModuleId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Module).AutoInclude();

        base.ConfigureModel(builder);
    }
}