using Blazorise.Utilities;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Utils;

public static class ProgressUtils
{
    public static IAsyncDisposable DoProgress(Action<int> setProgress, Func<int> getProgress, Func<Task> triggerStateChange, CancellationToken ct)
    {
        var cts = new CancellationTokenSource();
        var cts2 = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);
        setProgress(0);
        var task = Task.Run(async () =>
        {
            while (!cts2.IsCancellationRequested)
            {
                if (getProgress() == 100) break;
                if (getProgress() > 70)
                {
                    setProgress(getProgress() + 1);
                    await triggerStateChange();
                    await Task.Delay(60, cts2.Token);
                }
                else
                {
                    setProgress(getProgress() + 5);
                    await triggerStateChange();
                    await Task.Delay(30, cts2.Token);
                }
            }
        }, cts2.Token);
        return AsyncDisposable.Create(async () =>
        {
            await cts2.CancelAsync();
            await Task.WhenAny(task);
            task.Dispose();
            cts.Dispose();
            cts2.Dispose();
        });
    }
}