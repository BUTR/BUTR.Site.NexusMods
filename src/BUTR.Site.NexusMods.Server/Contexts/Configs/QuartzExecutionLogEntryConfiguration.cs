using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Configs;

public class QuartzExecutionLogEntityConfiguration : BaseEntityConfiguration<QuartzExecutionLogEntity>
{
    public override void Configure(ModelBuilder builder)
    {
        builder.HasSequence<int>("quartz_log_id_seq", "quartz")
            .StartsAt(1)
            .IncrementsBy(1);

        base.Configure(builder);
    }

    protected override void ConfigureModel(EntityTypeBuilder<QuartzExecutionLogEntity> builder)
    {
        builder.Property(x => x.LogId).HasColumnName("quartz_log_id").HasDefaultValueSql("nextval('\"quartz\".\"quartz_log_id_seq\"')");
        builder.Property(x => x.RunInstanceId).HasColumnName("run_instance_id");
        builder.Property(x => x.JobName).HasColumnName("job_name").HasMaxLength(256);
        builder.Property(x => x.JobGroup).HasColumnName("job_group").HasMaxLength(256);
        builder.Property(x => x.TriggerName).HasColumnName("trigger_name").HasMaxLength(256);
        builder.Property(x => x.TriggerGroup).HasColumnName("trigger_group").HasMaxLength(256);
        builder.Property(x => x.FireTimeUtc).HasColumnName("fire_time_utc");
        builder.Property(x => x.JobRunTime).HasColumnName("job_run_time");

        builder.Property(x => x.ScheduleFireTimeUtc).HasColumnName("schedule_fire_time_utc");

        builder.Property(x => x.RetryCount).HasColumnName("retry_count");

        builder.Property(x => x.IsSuccess).HasColumnName("is_success");
        builder.Property(x => x.IsException).HasColumnName("is_exception");
        builder.Property(x => x.IsVetoed).HasColumnName("is_vetoed");

        builder.Property(x => x.ErrorMessage).HasColumnName("error_message");
        builder.Property(x => x.ExecutionLogDetail).HasColumnName("log_detail").HasColumnType("jsonb");
        builder.Property(x => x.Result).HasColumnName("result");
        builder.Property(x => x.ReturnCode).HasColumnName("return_code");

        builder.Property(x => x.DateAddedUtc).HasColumnName("date_added_utc");
        builder.Property(x => x.MachineName).HasColumnName("machie_name");

        builder.ToTable("quartz_log", "quartz").HasKey(x => new
        {
            x.RunInstanceId,
            x.JobName,
            x.JobGroup,
            x.TriggerName,
            x.TriggerGroup,
            x.FireTimeUtc
        });

        base.ConfigureModel(builder);
    }
}