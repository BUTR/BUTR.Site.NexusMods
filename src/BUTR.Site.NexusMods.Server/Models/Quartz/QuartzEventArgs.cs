using System;
using System.Threading;

namespace BUTR.Site.NexusMods.Server.Models.Quartz;

public class QuartzEventArgs<TArgs> : EventArgs
{
    public TArgs Args { get; init; }
    public CancellationToken CancelToken { get; init; }

    public QuartzEventArgs(TArgs args, CancellationToken cancelToken = default)
    {
        Args = args;
        CancelToken = cancelToken;
    }
}