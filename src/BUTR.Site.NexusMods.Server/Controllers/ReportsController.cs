using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), AllowAnonymous, TenantRequired]
public sealed class ReportsController : ApiControllerBase
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
    public async Task<ActionResult<string>> GetAllAsync(CrashReportFileId id, CancellationToken ct) => Ok(await _crashReporterClient.GetCrashReportAsync(id, ct));

    [HttpGet("BlankRequest")]
    [Produces("text/plain")]
    public ApiResult BlankRequest() => Utils.Http.ApiResults.ApiResult.FromError(null);
}