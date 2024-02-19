using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Models.Quartz;

using Quartz;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public interface IQuartzEventProviderService : IJobListener, ITriggerListener
{
    event EventHandler<QuartzEventArgs<IJobExecutionContext>>? OnJobToBeExecuted;
    event EventHandler<QuartzEventArgs<IJobExecutionContext>>? OnJobExecutionVetoed;
    event EventHandler<JobWasExecutedEventArgs>? OnJobWasExecuted;
    event EventHandler<TriggerEventArgs>? OnTriggerComplete;
    event EventHandler<TriggerEventArgs>? OnTriggerFired;
}

[SingletonService<IQuartzEventProviderService>]
public class QuartzEventProviderService : IQuartzEventProviderService
{
    public event EventHandler<QuartzEventArgs<IJobExecutionContext>>? OnJobToBeExecuted;
    public event EventHandler<QuartzEventArgs<IJobExecutionContext>>? OnJobExecutionVetoed;
    public event EventHandler<JobWasExecutedEventArgs>? OnJobWasExecuted;
    public event EventHandler<TriggerEventArgs>? OnTriggerComplete;
    public event EventHandler<TriggerEventArgs>? OnTriggerFired;

    public string Name => nameof(QuartzEventProviderService);

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken ct)
    {
        OnJobToBeExecuted?.Invoke(this, new(context, ct));
        return Task.CompletedTask;
    }
    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken ct)
    {
        OnJobExecutionVetoed?.Invoke(this, new(context, ct));
        return Task.CompletedTask;
    }
    public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken ct)
    {
        OnJobWasExecuted?.Invoke(this, new(context, jobException, ct)
        {
            JobException = jobException
        });
        return Task.CompletedTask;
    }


    public Task TriggerFired(ITrigger trigger, IJobExecutionContext context, CancellationToken ct)
    {
        OnTriggerFired?.Invoke(this, new(trigger, context, ct));
        return Task.CompletedTask;
    }
    public Task TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode, CancellationToken ct)
    {
        OnTriggerComplete?.Invoke(this, new(trigger, context, ct));
        return Task.CompletedTask;
    }

    public Task<bool> VetoJobExecution(ITrigger trigger, IJobExecutionContext context, CancellationToken ct)
    {
        return Task.FromResult(false);
    }

    public Task TriggerMisfired(ITrigger trigger, CancellationToken ct) => Task.CompletedTask;
}