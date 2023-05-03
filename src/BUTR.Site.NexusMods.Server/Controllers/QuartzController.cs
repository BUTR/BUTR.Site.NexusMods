using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme, Roles = $"{ApplicationRoles.Administrator}")]
public sealed class QuartzController : ControllerExtended
{
    private readonly ILogger _logger;
    private readonly AppDbContext _dbContext;
    private readonly ISchedulerFactory _schedulerFactory;

    public QuartzController(ILogger<QuartzController> logger, AppDbContext dbContext, ISchedulerFactory schedulerFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));
    }

    [HttpPost("HistoryPaginated")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<PagingData<QuartzExecutionLogEntity>?>>> HistoryPaginated([FromBody] PaginatedQuery query, CancellationToken ct)
    {
        var paginated = await _dbContext.Set<QuartzExecutionLogEntity>()
            .Where(x => x.LogType == QuartzLogType.ScheduleJob)
            .PaginatedAsync(query, 100, new() { Property = nameof(QuartzExecutionLogEntity.DateAddedUtc), Type = SortingType.Descending }, ct);

        return APIResponse(new PagingData<QuartzExecutionLogEntity>
        {
            Items = paginated.Items.ToAsyncEnumerable(),
            Metadata = paginated.Metadata
        });
    }

    [HttpGet("Delete")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> Delete([FromQuery] long logId)
    {
        QuartzExecutionLogEntity? ApplyChanges(QuartzExecutionLogEntity? existing) => existing switch
        {
            _ => null
        };
        if (await _dbContext.AddUpdateRemoveAndSaveAsync<QuartzExecutionLogEntity>(x => x.LogId == logId, ApplyChanges))
            return APIResponse("Deleted successful!");

        return APIResponseError<string>("Failed to delete!");
    }

    [HttpGet("TriggerJob")]
    [Produces("application/json")]
    public async Task<ActionResult> TriggerJob(string jobId, CancellationToken ct)
    {
        var scheduler = await _schedulerFactory.GetScheduler(ct);
        _ = scheduler.TriggerJob(new JobKey(jobId), CancellationToken.None);
        return Ok();
    }
}