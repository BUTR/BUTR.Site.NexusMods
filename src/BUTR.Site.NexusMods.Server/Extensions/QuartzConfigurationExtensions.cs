using Quartz;

using System;

namespace BUTR.Site.NexusMods.Server.Extensions;

internal static class QuartzConfigurationExtensions
{
    public static void AddJob<TJob>(this IServiceCollectionQuartzConfigurator quartzConfigurator, CronScheduleBuilder cronScheduleBuilder) where TJob : IJob
    {
        var jobName = typeof(TJob).Name;
        var jobKey = JobKey.Create(jobName);
        quartzConfigurator.AddJob<TJob>(opt => opt.WithIdentity(jobKey));
        quartzConfigurator.AddTrigger(opt => opt
            .ForJob(jobKey)
            .WithIdentity($"trigger-{jobName}")
            .WithCronSchedule(cronScheduleBuilder));
#if DEBUG
        quartzConfigurator.AddTrigger(opt => opt
            .ForJob(jobKey)
            .WithIdentity($"debug-trigger-{jobName}")
            .StartNow());
#endif
    }
    public static void AddJobAtStartup<TJob>(this IServiceCollectionQuartzConfigurator quartzConfigurator) where TJob : IJob
    {
        var jobName = typeof(TJob).Name;
        var jobKey = JobKey.Create(jobName);
        quartzConfigurator.AddJob<TJob>(opt => opt.WithIdentity(jobKey));
        quartzConfigurator.AddTrigger(opt => opt
            .ForJob(jobKey)
            .WithIdentity($"trigger-{jobName}")
            .StartNow());
    }
    public static void AddJob<TJob>(this IServiceCollectionQuartzConfigurator quartzConfigurator) where TJob : IJob
    {
        var jobName = typeof(TJob).Name;
        var jobKey = JobKey.Create(jobName);
        quartzConfigurator.AddJob<TJob>(opt => opt.WithIdentity(jobKey));
        quartzConfigurator.AddTrigger(opt => opt
            .ForJob(jobKey)
            .WithIdentity($"trigger-{jobName}")
            .StartAt(DateTimeOffset.MaxValue));
    }
}