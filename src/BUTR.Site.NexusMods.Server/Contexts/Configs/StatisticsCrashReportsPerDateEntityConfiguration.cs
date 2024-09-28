using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class StatisticsCrashReportsPerDateEntityConfiguration : BaseEntityConfigurationWithTenant<StatisticsCrashReportsPerDateEntity>
{
    public StatisticsCrashReportsPerDateEntityConfiguration(ITenantContextAccessor tenantContextAccessor) : base(tenantContextAccessor) { }

    protected override void ConfigureModel(EntityTypeBuilder<StatisticsCrashReportsPerDateEntity> builder)
    {
        builder.UseTpcMappingStrategy();
        builder.Property(x => x.Date).HasColumnName("date");
        builder.Property(x => x.GameVersion).HasColumnName("game_version").HasVogenConversion();
        builder.Property(x => x.NexusModsModId).HasColumnName("nexusmods_id").HasVogenConversion();
        builder.Property(x => x.ModuleId).HasColumnName("module_id").HasVogenConversion();
        builder.Property(x => x.ModuleVersion).HasColumnName("module_version").HasVogenConversion();
        builder.Property(x => x.Count).HasColumnName("count");
        builder.HasKey(x => new
        {
            x.TenantId,
            x.Date,
            x.GameVersion,
            x.NexusModsModId,
            x.ModuleId,
            x.ModuleVersion,
        });

        base.ConfigureModel(builder);
    }
}

public class StatisticsCrashReportsPerDayEntityEntityConfiguration : BaseEntityConfiguration<StatisticsCrashReportsPerDayEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<StatisticsCrashReportsPerDayEntity> builder)
    {
        builder.ToTable("crash_report_per_day", "statistics");

        base.ConfigureModel(builder);
    }
}

public class StatisticsCrashReportsPerMonthEntityConfiguration : BaseEntityConfiguration<StatisticsCrashReportsPerMonthEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<StatisticsCrashReportsPerMonthEntity> builder)
    {
        builder.ToTable("crash_report_per_month", "statistics");

        base.ConfigureModel(builder);
    }
}