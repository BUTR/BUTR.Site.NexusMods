using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using Quartz;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Jobs;

public sealed class QuartzLogHistoryManagerExecutionLogsJob : IJob
{
    private readonly AppDbContext _dbContext;

    public QuartzLogHistoryManagerExecutionLogsJob(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var count = await _dbContext.Set<QuartzExecutionLogEntity>().Where(x => DateTimeOffset.UtcNow - x.DateAddedUtc > TimeSpan.FromDays(30)).ExecuteDeleteAsync();
            context.Result = $"Deleted {count} record(s)";
            context.SetIsSuccess(true);
        }
        catch (Exception ex)
        {
            throw new JobExecutionException("Failed to delete execution logs", ex);
        }
    }
}