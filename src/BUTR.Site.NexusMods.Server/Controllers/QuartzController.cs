using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.APIResponses;
using BUTR.Site.NexusMods.Server.Utils.BindingSources;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), ButrNexusModsAuthorization(Roles = $"{ApplicationRoles.Administrator}"), TenantNotRequired]
public sealed class QuartzController : ControllerExtended
{
    private readonly ILogger _logger;
    private readonly IAppDbContextRead _dbContextRead;
    private readonly IAppDbContextWrite _dbContextWrite;
    private readonly ISchedulerFactory _schedulerFactory;

    public QuartzController(ILogger<QuartzController> logger, IAppDbContextRead dbContextRead, IAppDbContextWrite dbContextWrite, ISchedulerFactory schedulerFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContextRead = dbContextRead ?? throw new ArgumentNullException(nameof(dbContextRead));
        _dbContextWrite = dbContextWrite ?? throw new ArgumentNullException(nameof(dbContextWrite));
        _schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));
    }

    [HttpPost("HistoryPaginated")]
    [Produces("application/json")]
    public async Task<APIResponseActionResult<PagingData<QuartzExecutionLogEntity>?>> HistoryPaginatedAsync([FromBody] PaginatedQuery query, CancellationToken ct)
    {
        var paginated = await _dbContextRead.QuartzExecutionLogs.Prepare()
            .PaginatedAsync(query, 100, new() { Property = nameof(QuartzExecutionLogEntity.DateAddedUtc), Type = SortingType.Descending }, ct);

        return APIPagingResponse(paginated);
    }

    [HttpGet("Delete")]
    [Produces("application/json")]
    public async Task<APIResponseActionResult<string?>> DeleteAsync([FromQuery] long logId)
    {
        if (await _dbContextWrite.QuartzExecutionLogs.Where(x => x.LogId == logId).ExecuteDeleteAsync() > 0)
            return APIResponse("Deleted successful!");

        return APIResponseError<string>("Failed to delete!");
    }

    [HttpGet("TriggerJob")]
    [Produces("application/json")]
    public async Task<APIResponseActionResult<DateTimeOffset>> TriggerJobAsync(string jobId, [BindUserId] NexusModsUserId userId, CancellationToken ct)
    {
        var userName = HttpContext.GetName();

        var jobKey = JobKey.Create(jobId);
        var trigger = TriggerBuilder.Create()
            .WithIdentity($"User:{userId}:{userName}")
            .ForJob(jobKey)
            .StartNow()
            .Build();

        var scheduler = await _schedulerFactory.GetScheduler(ct);
        var startTime = await scheduler.ScheduleJob(trigger, CancellationToken.None);
        return APIResponse(startTime);
    }
}