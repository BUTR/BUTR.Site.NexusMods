using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BUTR.Site.NexusMods.Server.Contexts.Config;

public class QuartzExecutionLogEntityConfiguration : BaseEntityConfiguration<QuartzExecutionLogEntity>
{
    protected override void ConfigureModel(EntityTypeBuilder<QuartzExecutionLogEntity> builder)
    {
        builder.ToTable("quartz_execution_log_entity").HasKey(p => p.LogId).HasName("quartz_execution_log_entity_pkey");

        builder.Property(p => p.LogId).HasColumnName("log_id").ValueGeneratedOnAdd();

        builder.Property(p => p.RunInstanceId).HasColumnName("run_instance_id");

        builder.Property(p => p.LogType).HasColumnName("log_type").HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(p => p.JobName).HasColumnName("job_name").HasMaxLength(256);
        builder.Property(p => p.JobGroup).HasColumnName("job_group").HasMaxLength(256);
        builder.Property(p => p.TriggerName).HasColumnName("trigger_name").HasMaxLength(256);
        builder.Property(p => p.TriggerGroup).HasColumnName("trigger_group").HasMaxLength(256);

        builder.Property(p => p.ScheduleFireTimeUtc).HasColumnName("_schedule_fire_time_utc");
        builder.Property(p => p.FireTimeUtc).HasColumnName("fire_time_utc");

        builder.Property(p => p.JobRunTime).HasColumnName("job_run_time");

        builder.Property(p => p.RetryCount).HasColumnName("retry_count");

        builder.Property(p => p.Result).HasColumnName("result");

        builder.Property(p => p.ErrorMessage).HasColumnName("error_message");

        builder.Property(p => p.IsVetoed).HasColumnName("is_vetoed");
        builder.Property(p => p.IsException).HasColumnName("is_exception");
        builder.Property(p => p.IsSuccess).HasColumnName("is_success");

        builder.Property(p => p.ReturnCode).HasColumnName("return_code");

        builder.Property(p => p.DateAddedUtc).HasColumnName("date_added_utc").IsRequired();
        builder.Property(p => p.ExecutionLogDetail).HasColumnName("execution_log_detail").HasColumnType("jsonb");

        builder.HasIndex(p => p.RunInstanceId);
        builder.HasIndex(p => new { p.DateAddedUtc, p.LogType });
        builder.HasIndex(p => new { p.TriggerName, p.TriggerGroup, p.JobName, p.JobGroup, p.DateAddedUtc });
    }
}