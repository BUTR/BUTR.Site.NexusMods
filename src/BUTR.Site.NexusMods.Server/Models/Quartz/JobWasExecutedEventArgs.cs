using Quartz;

using System;
using System.Threading;

namespace BUTR.Site.NexusMods.Server.Models.Quartz;

public class JobWasExecutedEventArgs : EventArgs
{
    public IJobExecutionContext JobExecutionContext { get; init; }
    public JobExecutionException? JobException { get; init; }
    public CancellationToken CancelToken { get; set; }

    public JobWasExecutedEventArgs(IJobExecutionContext context, JobExecutionException? exception, CancellationToken cancelToken = default)
    {
        JobExecutionContext = context;
        JobException = exception;
        CancelToken = cancelToken;
    }
}