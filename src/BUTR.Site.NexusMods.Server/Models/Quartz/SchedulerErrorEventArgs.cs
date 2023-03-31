using Quartz;

using System;
using System.Threading;

namespace BUTR.Site.NexusMods.Server.Models.Quartz;

public class SchedulerErrorEventArgs : EventArgs
{
    public string ErrorMessage { get; init; } = null!;
    public SchedulerException Exception { get; init; } = null!;
    public CancellationToken CancelToken { get; init; }
}