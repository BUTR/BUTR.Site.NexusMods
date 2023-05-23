using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Models.Quartz;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

internal sealed class QuartzListenerBackgroundService : BackgroundService
{
    private const int RESULT_MAX_LENGTH = 8000;
    private const int MAX_BATCH_SIZE = 50;

    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly QuartzEventProviderService _quartzEventProviderService;
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly Channel<Func<AppDbContext, CancellationToken, ValueTask>> _taskQueue;

    public QuartzListenerBackgroundService(ILogger<QuartzListenerBackgroundService> logger, IServiceProvider serviceProvider, QuartzEventProviderService quartzEventProviderService, ISchedulerFactory schedulerFactory)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _quartzEventProviderService = quartzEventProviderService;
        _schedulerFactory = schedulerFactory;
        _taskQueue = Channel.CreateUnbounded<Func<AppDbContext, CancellationToken, ValueTask>>(new UnboundedChannelOptions { SingleReader = true });

        _quartzEventProviderService.OnJobToBeExecuted += OnJobToBeExecuted;
        _quartzEventProviderService.OnJobWasExecuted += OnJobWasExecuted;
        _quartzEventProviderService.OnJobExecutionVetoed += OnJobExecutionVetoed;
        _quartzEventProviderService.OnJobDeleted += OnJobDeleted;
        _quartzEventProviderService.OnJobInterrupted += OnJobInterrupted;
        _quartzEventProviderService.OnSchedulerError += OnSchedulerError;
        _quartzEventProviderService.OnTriggerMisfired += OnTriggerMisfired;
        _quartzEventProviderService.OnTriggerPaused += OnTriggerPaused;
        _quartzEventProviderService.OnTriggerResumed += OnTriggerResumed;
        _quartzEventProviderService.OnTriggerFinalized += OnTriggerFinalized;
        _quartzEventProviderService.OnJobScheduled += OnJobScheduled;
    }

    public override void Dispose()
    {
        _quartzEventProviderService.OnJobToBeExecuted -= OnJobToBeExecuted;
        _quartzEventProviderService.OnJobWasExecuted -= OnJobWasExecuted;
        _quartzEventProviderService.OnJobExecutionVetoed -= OnJobExecutionVetoed;
        _quartzEventProviderService.OnJobDeleted -= OnJobDeleted;
        _quartzEventProviderService.OnJobInterrupted -= OnJobInterrupted;
        _quartzEventProviderService.OnSchedulerError -= OnSchedulerError;
        _quartzEventProviderService.OnTriggerMisfired -= OnTriggerMisfired;
        _quartzEventProviderService.OnTriggerPaused -= OnTriggerPaused;
        _quartzEventProviderService.OnTriggerResumed -= OnTriggerResumed;
        _quartzEventProviderService.OnTriggerFinalized -= OnTriggerFinalized;
        _quartzEventProviderService.OnJobScheduled -= OnJobScheduled;

        base.Dispose();
    }

    private void OnJobScheduled(object? sender, EventArgs<ITrigger> e)
    {
        var jKey = e.Args.JobKey;
        var tKey = e.Args.Key;
        var log = new QuartzExecutionLogEntity
        {
            JobName = jKey.Name,
            JobGroup = jKey.Group,
            TriggerName = tKey.Name,
            TriggerGroup = tKey.Group,
            LogType = QuartzLogType.Trigger,
            Result = "Job scheduled"
        };
        QueueInsertTask(log);
    }

    private void OnTriggerFinalized(object? sender, EventArgs<ITrigger> e)
    {
        var jKey = e.Args.JobKey;
        var tKey = e.Args.Key;
        var log = new QuartzExecutionLogEntity
        {
            JobName = jKey.Name,
            JobGroup = jKey.Group,
            TriggerName = tKey.Name,
            TriggerGroup = tKey.Group,
            LogType = QuartzLogType.Trigger,
            Result = "Trigger ended"
        };
        QueueInsertTask(log);
    }

    private void OnTriggerResumed(object? sender, EventArgs<TriggerKey> e)
    {
        var tKey = e.Args;
        var log = new QuartzExecutionLogEntity
        {
            TriggerName = tKey.Name,
            TriggerGroup = tKey.Group,
            LogType = QuartzLogType.Trigger,
            Result = "Trigger resumed"
        };
        QueueGetJobKeyAndInsertTask(log);
    }

    private void OnTriggerPaused(object? sender, EventArgs<TriggerKey> e)
    {
        var tKey = e.Args;
        var log = new QuartzExecutionLogEntity
        {
            TriggerName = tKey.Name,
            TriggerGroup = tKey.Group,
            LogType = QuartzLogType.Trigger,
            Result = "Trigger paused"
        };
        QueueGetJobKeyAndInsertTask(log);
    }

    private void OnTriggerMisfired(object? sender, EventArgs<ITrigger> e)
    {
        var jKey = e.Args.JobKey;
        var tKey = e.Args.Key;
        var log = new QuartzExecutionLogEntity
        {
            LogType = QuartzLogType.Trigger,
            JobName = jKey.Name,
            JobGroup = jKey.Group,
            TriggerName = tKey.Name,
            TriggerGroup = tKey.Group,
            Result = "Trigger misfired"
        };
        QueueInsertTask(log);
    }

    private void OnSchedulerError(object? sender, SchedulerErrorEventArgs e)
    {
        var log = new QuartzExecutionLogEntity
        {
            LogType = QuartzLogType.System,
            IsException = true,
            ErrorMessage = e.ErrorMessage,
            ExecutionLogDetail = new()
            {
                ErrorStackTrace = NonNullStackTrace(e.Exception)
            }
        };
        QueueInsertTask(log);
    }

    private void OnJobInterrupted(object? sender, EventArgs<JobKey> e)
    {
        var jKey = e.Args;
        var log = new QuartzExecutionLogEntity
        {
            JobName = jKey.Name,
            JobGroup = jKey.Group,
            LogType = QuartzLogType.System,
            Result = "Job interrupted"
        };
        QueueInsertTask(log);
    }

    private void OnJobDeleted(object? sender, EventArgs<JobKey> e)
    {
        var jKey = e.Args;
        var log = new QuartzExecutionLogEntity
        {
            JobName = jKey.Name,
            JobGroup = jKey.Group,
            LogType = QuartzLogType.System,
            Result = "Job deleted"
        };
        QueueInsertTask(log);
    }

    private void OnJobExecutionVetoed(object? sender, EventArgs<IJobExecutionContext> e)
    {
        var log = CreateScheduleJobLogEntry(e.Args, defaultIsSuccess: false);
        log.IsVetoed = true;
        QueueUpdateTask(log);
    }

    private void OnJobWasExecuted(object? sender, JobWasExecutedEventArgs e)
    {
        QueueUpdateTask(CreateScheduleJobLogEntry(e.JobExecutionContext, e.JobException, true));
    }

    private void OnJobToBeExecuted(object? sender, EventArgs<IJobExecutionContext> e)
    {
        QueueInsertTask(CreateScheduleJobLogEntry(e.Args));
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await MarkIncompleteExecutionAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessTaskAsync(stoppingToken);
        }
    }

    internal async Task MarkIncompleteExecutionAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var isSuccessNullJobs = repo.Set<QuartzExecutionLogEntity>().Where(x => !x.IsSuccess.HasValue && x.LogType == QuartzLogType.ScheduleJob);
            foreach (var log in isSuccessNullJobs)
            {
                log.IsSuccess = false;
                log.ErrorMessage = "Incomplete execution.";
                log.JobRunTime = null;
            }
            await repo.SaveChangesAsync(CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // Prevent throwing if stoppingToken was signaled
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating executing status to incomplete status.");
        }
    }

    internal async Task ProcessTaskAsync(CancellationToken stoppingToken = default)
    {
        var batch = await GetBatchAsync(stoppingToken);

        _logger.LogInformation("Got a batch with {taskCount} task(s). Saving to data store.",
            batch.Count);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            foreach (var workItem in batch)
            {
                await workItem(repo, stoppingToken);
            }

            try
            {
                await repo.SaveChangesAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving execution logs.");
            }
        }
        catch (OperationCanceledException)
        {
            // Prevent throwing if stoppingToken was signaled
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred executing task work item.");
        }
    }


    private QuartzExecutionLogEntity CreateScheduleJobLogEntry(IJobExecutionContext context, JobExecutionException? jobException = null, bool? defaultIsSuccess = null)
    {
        var log = new QuartzExecutionLogEntity
        {
            RunInstanceId = context.FireInstanceId,
            JobGroup = context.JobDetail.Key.Group,
            JobName = context.JobDetail.Key.Name,
            TriggerName = context.Trigger.Key.Name,
            TriggerGroup = context.Trigger.Key.Group,
            FireTimeUtc = context.FireTimeUtc,
            ScheduleFireTimeUtc = context.ScheduledFireTimeUtc,
            RetryCount = context.RefireCount,
            JobRunTime = context.JobRunTime,
            LogType = QuartzLogType.ScheduleJob
        };
        var logDetail = new QuartzExecutionLogDetailEntity();

        log.ReturnCode = context.GetReturnCode();
        log.IsSuccess = context.GetIsSuccess() ?? defaultIsSuccess;

        var execDetail = context.GetExecutionDetails();
        if (!string.IsNullOrEmpty(execDetail))
        {
            logDetail.ExecutionDetails = execDetail;
            log.ExecutionLogDetail = logDetail;
        }

        if (jobException != null)
        {
            log.ErrorMessage = jobException.Message;
            log.ExecutionLogDetail = logDetail;
            logDetail.ErrorCode = jobException.HResult;
            logDetail.ErrorStackTrace = jobException.ToString();
            logDetail.ErrorHelpLink = jobException.HelpLink;

            log.ReturnCode ??= jobException.HResult.ToString();

            log.IsException = true;
            log.IsSuccess = false;
        }
        else
        {
            if (context.Result != null)
            {
                var result = Convert.ToString(context.Result, CultureInfo.InvariantCulture);
                log.Result = result?.Substring(0, Math.Min(result.Length, RESULT_MAX_LENGTH));
            }
        }

        return log;
    }

    private async Task<List<Func<AppDbContext, CancellationToken, ValueTask>>> GetBatchAsync(CancellationToken cancellationToken)
    {
        await _taskQueue.Reader.WaitToReadAsync(cancellationToken);

        var batch = new List<Func<AppDbContext, CancellationToken, ValueTask>>();

        while (batch.Count < MAX_BATCH_SIZE && _taskQueue.Reader.TryRead(out var dbTask))
        {
            batch.Add(dbTask);
        }

        return batch;
    }

    private void QueueUpdateTask(QuartzExecutionLogEntity log)
    {
        QueueTask(async (repo, ct) =>
        {
            try
            {
                QuartzExecutionLogEntity? ApplyChanges(QuartzExecutionLogEntity? existing) => existing switch
                {
                    null => log,
                    var entity => entity with
                    {
                        ExecutionLogDetail = log.ExecutionLogDetail,
                        ErrorMessage = log.ErrorMessage,
                        IsVetoed = log.IsVetoed,
                        JobRunTime = log.JobRunTime,
                        Result = log.Result,
                        IsException = log.IsException,
                        IsSuccess = log.IsSuccess,
                        ReturnCode = log.ReturnCode,
                    }
                };
                await repo.AddUpdateRemoveAsync<QuartzExecutionLogEntity>(x => x.RunInstanceId == log.RunInstanceId, ApplyChanges, ct: CancellationToken.None);
            }
            catch (Exception ex)
            {
                if (log.LogType == QuartzLogType.ScheduleJob)
                {
                    _logger.LogError(ex,
                        "Error occurred while updating execution log with job key [{jobGroup}.{jobName}] run instance id [{runInstanceId}].",
                        log.JobGroup, log.JobName, log.RunInstanceId);
                }
                else
                {
                    _logger.LogError(ex,
                        "Error occurred while updating {logType} execution log with job key [{jobGroup}.{jobName}] trigger key [{triggerGroup}.{triggerName}].",
                        log.LogType, log.JobGroup, log.JobName, log.TriggerGroup, log.TriggerName);
                }
            }
        });
    }

    private void QueueGetJobKeyAndInsertTask(QuartzExecutionLogEntity log)
    {
        QueueTask(async (repo, ct) =>
        {
            try
            {
                if (log.JobName == null && log is { TriggerName: { }, TriggerGroup: { } })
                {
                    // when there is no job name but has trigger name
                    // try to determine the job name
                    var scheduler = await _schedulerFactory.GetScheduler(ct);
                    var trigger = await scheduler.GetTrigger(new TriggerKey(log.TriggerName, log.TriggerGroup), ct);
                    if (trigger != null)
                    {
                        log.JobName = trigger.JobKey.Name;
                        log.JobGroup = trigger.JobKey.Group;
                    }
                }

                await repo.Set<QuartzExecutionLogEntity>().AddAsync(log, CancellationToken.None);
            }
            catch (Exception ex)
            {
                if (log.LogType == QuartzLogType.ScheduleJob)
                {
                    _logger.LogError(ex, "Error occurred while adding execution log with job key [{jobGroup}.{jobName}] run instance id [{runInstanceId}].",
                        log.JobGroup, log.JobName, log.RunInstanceId);
                }
                else
                {
                    _logger.LogError(ex, "Error occurred while adding {logType} execution log with job key [{jobGroup}.{jobName}] trigger key [{triggerGroup}.{triggerName}].",
                        log.LogType, log.JobGroup, log.JobName, log.TriggerGroup, log.TriggerName);
                }
            }
        });
    }


    private void QueueInsertTask(QuartzExecutionLogEntity log)
    {
        QueueTask(async (repo, ct) =>
        {
            try
            {
                await repo.Set<QuartzExecutionLogEntity>().AddAsync(log, CancellationToken.None);
            }
            catch (Exception ex)
            {
                if (log.LogType == QuartzLogType.ScheduleJob)
                {
                    _logger.LogError(ex, "Error occurred while adding execution log with job key [{jobGroup}.{jobName}] run instance id [{runInstanceId}].",
                        log.JobGroup, log.JobName, log.RunInstanceId);
                }
                else
                {
                    _logger.LogError(ex, "Error occurred while adding {logType} execution log with job key [{jobGroup}.{jobName}] trigger key [{triggerGroup}.{triggerName}].",
                        log.LogType, log.JobGroup, log.JobName, log.TriggerGroup, log.TriggerName);
                }
            }
        });
    }

    private void QueueTask(Func<AppDbContext, CancellationToken, ValueTask> task)
    {
        if (!_taskQueue.Writer.TryWrite(task))
        {
            // Should not happen since it's unbounded Channel. It 'should' only fail if we call writer.Complete()
            throw new InvalidOperationException("Failed to write the log message");
        }
    }

    /// <summary>
    /// Return closest non null stack trace of exception.
    /// Loop until null InnerException to get stack trace.
    /// </summary>
    /// <param name="exception"></param>
    /// <returns>null if inner exceptions does not have stack trace</returns>
    private static string? NonNullStackTrace(Exception? exception)
    {
        var currentException = exception;
        while (currentException?.StackTrace == null)
        {
            currentException = currentException?.InnerException;
            if (currentException == null)
                break;
        }
        return currentException?.StackTrace;
    }
}