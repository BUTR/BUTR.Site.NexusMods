using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Models.Quartz;
using BUTR.Site.NexusMods.Server.Repositories;
using BUTR.Site.NexusMods.Server.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.BackgroundServices;

[HostedService]
internal sealed class QuartzListenerBackgroundService : BackgroundService
{
    private const int RESULT_MAX_LENGTH = 8000;
    private const int MAX_BATCH_SIZE = 50;

    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IQuartzEventProviderService _quartzEventProviderService;
    private readonly Channel<Func<IUnitOfWrite, CancellationToken, ValueTask>> _taskQueue;

    public QuartzListenerBackgroundService(ILogger<QuartzListenerBackgroundService> logger, IServiceScopeFactory serviceScopeFactory, IQuartzEventProviderService quartzEventProviderService)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _quartzEventProviderService = quartzEventProviderService;
        _taskQueue = Channel.CreateUnbounded<Func<IUnitOfWrite, CancellationToken, ValueTask>>(new UnboundedChannelOptions { SingleReader = true });

        _quartzEventProviderService.OnJobToBeExecuted += OnJobToBeExecuted;
        _quartzEventProviderService.OnJobWasExecuted += OnJobWasExecuted;
        _quartzEventProviderService.OnJobExecutionVetoed += OnJobExecutionVetoed;
    }

    public override void Dispose()
    {
        _quartzEventProviderService.OnJobToBeExecuted -= OnJobToBeExecuted;
        _quartzEventProviderService.OnJobWasExecuted -= OnJobWasExecuted;
        _quartzEventProviderService.OnJobExecutionVetoed -= OnJobExecutionVetoed;

        base.Dispose();
    }

    private void OnJobToBeExecuted(object? sender, QuartzEventArgs<IJobExecutionContext> e) =>
        QueueInsertTask(CreateScheduleJobLogEntry(e.Args));

    private void OnJobWasExecuted(object? sender, JobWasExecutedEventArgs e)
    {
        QueueUpdateTask(CreateScheduleJobLogEntry(e.JobExecutionContext, e.JobException, true));
    }

    private void OnJobExecutionVetoed(object? sender, QuartzEventArgs<IJobExecutionContext> e) =>
        QueueUpdateTask(CreateScheduleJobLogEntry(e.Args, defaultIsSuccess: false) with
        {
            IsVetoed = true
        });


    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var unitOfWorkFactory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();

        await MarkIncompleteExecutionAsync(unitOfWorkFactory, ct);

        while (!ct.IsCancellationRequested)
            await ProcessTaskAsync(unitOfWorkFactory, ct);
    }

    private async Task MarkIncompleteExecutionAsync(IUnitOfWorkFactory unitOfWorkFactory, CancellationToken ct)
    {
        try
        {
            await using var unitOfWrite = unitOfWorkFactory.CreateUnitOfWrite(TenantId.None);

            await unitOfWrite.QuartzExecutionLogs.MarkIncompleteAsync(ct);
            await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // Prevent throwing if stoppingToken was signaled
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating executing status to incomplete status");
        }
    }

    private async Task ProcessTaskAsync(IUnitOfWorkFactory unitOfWorkFactory, CancellationToken ct = default)
    {
        var batch = await GetBatchAsync(ct);

        _logger.LogInformation("Got a batch with {TaskCount} task(s). Saving to data store", batch.Count);

        await using var unitOfWrite = unitOfWorkFactory.CreateUnitOfWrite(TenantId.None);
        try
        {
            foreach (var workItem in batch)
                await workItem(unitOfWrite, ct);
            await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // Prevent throwing if stoppingToken was signaled
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred executing task work item");
        }
    }

    private void QueueTask(Func<IUnitOfWrite, CancellationToken, ValueTask> task)
    {
        if (!_taskQueue.Writer.TryWrite(task))
        {
            // Should not happen since it's unbounded Channel. It 'should' only fail if we call writer.Complete()
            throw new InvalidOperationException("Failed to write the log message");
        }
    }

    private async Task<List<Func<IUnitOfWrite, CancellationToken, ValueTask>>> GetBatchAsync(CancellationToken ct)
    {
        await _taskQueue.Reader.WaitToReadAsync(ct);

        var batch = new List<Func<IUnitOfWrite, CancellationToken, ValueTask>>();

        while (batch.Count < MAX_BATCH_SIZE && _taskQueue.Reader.TryRead(out var dbTask))
        {
            batch.Add(dbTask);
        }

        return batch;
    }

    private void QueueUpdateTask(QuartzExecutionLogEntity log) => QueueTask(async (dbContext, ct) =>
    {
        try
        {
            await Task.Yield();
            var existingJob = await dbContext.QuartzExecutionLogs.FirstOrDefaultAsync(x =>
                x.RunInstanceId == log.RunInstanceId &&
                x.JobName == log.JobName &&
                x.JobGroup == log.JobGroup &&
                x.TriggerName == log.TriggerName &&
                x.TriggerGroup == log.TriggerGroup &&
                x.FireTimeUtc == log.FireTimeUtc, null, ct);
            dbContext.QuartzExecutionLogs.Update(existingJob!, log with { LogId = existingJob!.LogId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating execution log with job key [{JobGroup}.{JobName}] trigger key [{TriggerGroup}.{TriggerName}] run instance id [{RunInstanceId}]",
                log.JobGroup, log.JobName, log.TriggerGroup, log.TriggerName, log.RunInstanceId);
        }
    });

    private void QueueInsertTask(QuartzExecutionLogEntity log) => QueueTask(async (dbContext, ct) =>
    {
        try
        {
            await Task.Yield();
            dbContext.QuartzExecutionLogs.Add(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding execution log with job key [{JobGroup}.{JobName}] trigger key [{TriggerGroup}.{TriggerName}] run instance id [{RunInstanceId}]",
                log.JobGroup, log.JobName, log.TriggerGroup, log.TriggerName, log.RunInstanceId);
        }
    });

    private static QuartzExecutionLogEntity CreateScheduleJobLogEntry(IJobExecutionContext context, JobExecutionException? jobException = null, bool? defaultIsSuccess = null)
    {
        var result = context.Result != null ? Convert.ToString(context.Result, CultureInfo.InvariantCulture) : null;
        var isSuccess = jobException == null ? context.GetIsSuccess() ?? defaultIsSuccess : false;

        var logDetail = new QuartzExecutionLogDetailEntity
        {
            ExecutionDetails = context.GetExecutionDetails(),

            ErrorCode = jobException?.HResult,
            ErrorStackTrace = jobException?.ToString(),
            ErrorHelpLink = jobException?.HelpLink,
        };

        var log = new QuartzExecutionLogEntity
        {
            LogId = 0,
            RunInstanceId = context.FireInstanceId,
            JobName = context.JobDetail.Key.Name,
            JobGroup = context.JobDetail.Key.Group,
            TriggerName = context.Trigger.Key.Name,
            TriggerGroup = context.Trigger.Key.Group,
            FireTimeUtc = context.FireTimeUtc,
            JobRunTime = context.JobRunTime,

            ScheduleFireTimeUtc = context.ScheduledFireTimeUtc,

            RetryCount = context.RefireCount,

            IsSuccess = isSuccess,
            IsException = jobException == null ? !isSuccess : true,
            IsVetoed = null,

            ErrorMessage = jobException?.GetBaseException().Message,
            ExecutionLogDetail = logDetail,
            Result = result?.Substring(0, Math.Min(result.Length, RESULT_MAX_LENGTH)),
            ReturnCode = context.GetReturnCode() ?? jobException?.HResult.ToString(),
        };

        return log;
    }
}