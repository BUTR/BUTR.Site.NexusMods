using BUTR.Site.NexusMods.Server.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers
{


    [ApiController, Route("api/v1/[controller]"), AllowAnonymous]
    public class ReportsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly CrashReporterClient _crashReporterClient;

        public ReportsController(ILogger<ReportsController> logger, CrashReporterClient crashReporterClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _crashReporterClient = crashReporterClient ?? throw new ArgumentNullException(nameof(crashReporterClient));
        }

        [HttpGet("{id}.html")]
        [Produces("text/html")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAll(string id, CancellationToken ct) =>
            StatusCode(StatusCodes.Status200OK, await _crashReporterClient.GetCrashReportAsync(id, ct));
    }
}