using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Repositories;

using Microsoft.Extensions.DependencyInjection;

using Quartz;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Jobs;

public sealed class QuartzLogHistoryManagerExecutionLogsJob : IJob
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public QuartzLogHistoryManagerExecutionLogsJob(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope().WithTenant(TenantId.None);

        var unitOfWorkFactory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();
        await using var unitOfWrite = unitOfWorkFactory.CreateUnitOfWrite();

        var count = unitOfWrite.QuartzExecutionLogs
            .Remove(x => DateTimeOffset.UtcNow - x.DateAddedUtc > TimeSpan.FromDays(30));

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);

        context.Result = $"Deleted {count} execution logs";
        context.SetIsSuccess(true);
    }
}