using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers
{
    [ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme, Roles = $"{ApplicationRoles.Administrator}")]
    public sealed class QuartzController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly InMemoryQuartzJobHistory _jobHistory;
        private readonly ISchedulerFactory _schedulerFactory;

        public QuartzController(ILogger<ReportsController> logger, InMemoryQuartzJobHistory jobHistory, ISchedulerFactory schedulerFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jobHistory = jobHistory ?? throw new ArgumentNullException(nameof(jobHistory));
            _schedulerFactory = schedulerFactory;
        }

        [HttpGet("Status")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Dictionary<string, JobInfo>), StatusCodes.Status200OK)]
        public ActionResult GetAll()
        {
            return StatusCode(StatusCodes.Status200OK, _jobHistory.JobHistory);
        }

        [HttpGet("TriggerJob")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<ActionResult> TriggerJob(string jobId, CancellationToken ct)
        {
            var scheduler = await _schedulerFactory.GetScheduler(ct);
            _ = scheduler.TriggerJob(new JobKey(jobId), CancellationToken.None);
            return StatusCode(StatusCodes.Status200OK);
        }
    }
}