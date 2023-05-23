using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), AllowAnonymous]
public sealed class ReportsController : ControllerExtended
{
    private readonly ILogger _logger;
    private readonly CrashReporterClient _crashReporterClient;

    public ReportsController(ILogger<ReportsController> logger, CrashReporterClient crashReporterClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _crashReporterClient = crashReporterClient ?? throw new ArgumentNullException(nameof(crashReporterClient));
    }

    [HttpGet("Get/{id}.html")]
    [Produces("text/html")]
    public async Task<ActionResult<string>> GetAllAsync(string id, CancellationToken ct) => Ok(await _crashReporterClient.GetCrashReportAsync(id, ct));

    [HttpGet("BlankRequest")]
    [Produces("text/plain")]
    public ActionResult<APIStreamingResponse> BlankRequest() => Ok(string.Empty);
}