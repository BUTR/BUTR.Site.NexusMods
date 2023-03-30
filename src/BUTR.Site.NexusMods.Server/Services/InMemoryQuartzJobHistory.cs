using Quartz;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public sealed record JobInfo(string Id, DateTimeOffset StartTimeUtc, string Type, int RunTimeMilliseconds, Dictionary<string, string?> Metadata, string? Exception = null);

public sealed class InMemoryQuartzJobHistory : IJobListener
{
    public string Name => nameof(InMemoryQuartzJobHistory);

    public ConcurrentDictionary<string, JobInfo> JobHistory { get; } = new();

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken ct)
    {
        try
        {
            //var jobId = context.JobDetail.Key.ToString();
            var jobId = context.FireInstanceId;
            JobHistory[jobId] = new JobInfo(jobId, context.FireTimeUtc, context.JobDetail.JobType.Name, TimeSpan.Zero.Milliseconds, context.MergedJobDataMap.ToDictionary(x => x.Key, x => x.Value.ToString()));
        }
        catch (Exception) { }
        return Task.CompletedTask;
    }

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken ct)
    {
        try
        {
            //var jobId = context.JobDetail.Key.ToString();
            var jobId = context.FireInstanceId;
            JobHistory[jobId] = new JobInfo(jobId, context.FireTimeUtc, context.JobDetail.JobType.Name, context.JobRunTime.Milliseconds, context.MergedJobDataMap.ToDictionary(x => x.Key, x => x.Value.ToString()));
        }
        catch (Exception) { }
        return Task.CompletedTask;
    }

    public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken ct)
    {
        try
        {
            //var jobId = context.JobDetail.Key.ToString();
            var jobId = context.FireInstanceId;
            JobHistory[jobId] = new JobInfo(jobId, context.FireTimeUtc, context.JobDetail.JobType.Name, context.JobRunTime.Milliseconds, context.MergedJobDataMap.ToDictionary(x => x.Key, x => x.Value.ToString()), jobException?.ToString());
        }
        catch (Exception) { }
        return Task.CompletedTask;
    }
}