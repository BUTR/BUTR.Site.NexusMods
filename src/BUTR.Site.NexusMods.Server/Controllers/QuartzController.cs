using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Repositories;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.BindingSources;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), ButrNexusModsAuthorization(Roles = $"{ApplicationRoles.Administrator}"), TenantNotRequired]
public sealed class QuartzController : ApiControllerBase
{
    private readonly ILogger _logger;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly ISchedulerFactory _schedulerFactory;

    public QuartzController(ILogger<QuartzController> logger, IUnitOfWorkFactory unitOfWorkFactory, ISchedulerFactory schedulerFactory)
    {
        _logger = logger;
        _unitOfWorkFactory = unitOfWorkFactory;
        _schedulerFactory = schedulerFactory;
    }

    [HttpPost("Triggers")]
    public async Task<ApiResult<DateTimeOffset>> AddTriggerAsync([FromQuery, Required] string jobId, [BindUserId] NexusModsUserId userId, [BindUserName] NexusModsUserName userName, CancellationToken ct)
    {
        var jobKey = JobKey.Create(jobId);
        var trigger = TriggerBuilder.Create()
            .WithIdentity($"User:{userId}:{userName}")
            .ForJob(jobKey)
            .StartNow()
            .Build();

        var scheduler = await _schedulerFactory.GetScheduler(ct);
        var startTime = await scheduler.ScheduleJob(trigger, CancellationToken.None);
        return ApiResult(startTime);
    }

    [HttpPost("Jobs/Paginated")]
    public async Task<ApiResult<PagingData<QuartzExecutionLogEntity>?>> JobsPaginatedAsync([FromBody, Required] PaginatedQuery query, CancellationToken ct)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead(TenantId.None);

        var paginated = await unitOfRead.QuartzExecutionLogs
            .PaginatedAsync(query, 100, new() { Property = nameof(QuartzExecutionLogEntity.DateAddedUtc), Type = SortingType.Descending }, ct);

        return ApiPagingResult(paginated);
    }

    [HttpDelete("Jobs")]
    public async Task<ApiResult<string?>> RenoveJobAsync([FromQuery, Required] int logId)
    {
        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite(TenantId.None);

        if (unitOfWrite.QuartzExecutionLogs.Remove(x => x.LogId == logId) <= 0)
            return ApiBadRequest("Failed to delete!");

        return ApiResult("Deleted successful!");

    }
}