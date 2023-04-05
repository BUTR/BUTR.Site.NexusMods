using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers
{
    [ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme, Roles = $"{ApplicationRoles.Administrator}")]
    public sealed class QuartzController : ControllerExtended
    {
        public sealed record PaginatedQuery(uint Page, uint PageSize);


        private readonly ILogger _logger;
        private readonly AppDbContext _dbContext;
        private readonly ISchedulerFactory _schedulerFactory;

        public QuartzController(ILogger<ReportsController> logger, AppDbContext dbContext, ISchedulerFactory schedulerFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));
        }

        [HttpGet("HistoryPaginated")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(APIResponse<PagingData<QuartzExecutionLogEntity>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse<PagingData<QuartzExecutionLogEntity>?>>> HistoryPaginated([FromQuery] PaginatedQuery query, CancellationToken ct)
        {
            var page = query.Page;
            var pageSize = Math.Max(Math.Min(query.PageSize, 100), 5);

            var dbQuery = _dbContext.Set<QuartzExecutionLogEntity>()
                .Where(x => x.LogType == QuartzLogType.ScheduleJob)
                .OrderBy(x => x.DateAddedUtc);
            var paginated = await dbQuery.PaginatedAsync(page, pageSize, ct);

            return Result(APIResponse.From(new PagingData<QuartzExecutionLogEntity>
            {
                Items = paginated.Items.ToAsyncEnumerable(),
                Metadata = paginated.Metadata
            }));
        }

        [HttpGet("TriggerJob")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> TriggerJob(string jobId, CancellationToken ct)
        {
            var scheduler = await _schedulerFactory.GetScheduler(ct);
            _ = scheduler.TriggerJob(new JobKey(jobId), CancellationToken.None);
            return Ok();
        }
    }
}