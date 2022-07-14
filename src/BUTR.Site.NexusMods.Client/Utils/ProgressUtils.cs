using Blazorise.Utilities;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Utils
{
    public static class ProgressUtils
    {
        public static IAsyncDisposable DoProgress(Action<int> setProgress, Func<int> getProgress, Func<Task> triggerStateChange)
        {
            var cts = new CancellationTokenSource();
            setProgress(0);
            var task = Task.Run(async () =>
            {
                while (cts is not null && !cts.IsCancellationRequested)
                {
                    if (getProgress() == 100) break;
                    if (getProgress() > 70)
                    {
                        setProgress(getProgress() + 1);
                        await triggerStateChange();
                        await Task.Delay(60, cts.Token);
                    }
                    else
                    {
                        setProgress(getProgress() + 5);
                        await triggerStateChange();
                        await Task.Delay(30, cts.Token);
                    }
                }
            }, cts.Token);
            return AsyncDisposable.Create(async () =>
            {
                cts.Cancel();
                await Task.WhenAny(task);;
                task.Dispose();
                cts.Dispose();
            });
        }
    }
}