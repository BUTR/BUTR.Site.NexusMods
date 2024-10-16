using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.BindingSources;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), AllowAnonymous, TenantRequired]
public sealed class ReportsController : ApiControllerBase
{
    private readonly ILogger _logger;
    private readonly ICrashReporterClient _crashReporterClient;

    public ReportsController(ILogger<ReportsController> logger, ICrashReporterClient crashReporterClient)
    {
        _logger = logger;
        _crashReporterClient = crashReporterClient;
    }

    [HttpGet("{id}.html")]
    [Produces("text/html")]
    public async Task<ActionResult<string?>> GetAllAsync([BindTenant] TenantId tenant, CrashReportFileId id, CancellationToken ct)
    {
        return Ok(await _crashReporterClient.GetCrashReportAsync(tenant, id, ct));
    }

    // Just so we have ApiResult type in swagger.json
    [HttpGet("BlankRequest")]
    [Produces("text/plain")]
    public ApiResult BlankRequest() => ApiResultError("", StatusCodes.Status400BadRequest);
}