using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using Quartz;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Jobs;

public sealed class QuartzLogHistoryManagerExecutionLogsJob : IJob
{
    private readonly IAppDbContextWrite _dbContextWrite;

    public QuartzLogHistoryManagerExecutionLogsJob(IAppDbContextWrite dbContextWrite)
    {
        _dbContextWrite = dbContextWrite ?? throw new ArgumentNullException(nameof(dbContextWrite));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var ct = CancellationToken.None;

        var count = await _dbContextWrite.QuartzExecutionLogs
            .Where(x => DateTimeOffset.UtcNow - x.DateAddedUtc > TimeSpan.FromDays(30))
            .ExecuteDeleteAsync(ct);

        context.Result = $"Deleted {count} execution logs";
        context.SetIsSuccess(true);
    }
}