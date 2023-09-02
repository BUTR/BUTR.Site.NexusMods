using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
public sealed class GameSourceDiffController : ControllerExtended
{
    private const string basePath = "/source-api-diff";

    private readonly ILogger _logger;
    private readonly DiffProvider _diffProvider;

    public GameSourceDiffController(ILogger<GameSourceDiffController> logger, DiffProvider diffProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _diffProvider = diffProvider ?? throw new ArgumentNullException(nameof(diffProvider));
    }

    [HttpGet("List")]
    [Produces("application/json")]
    public ActionResult<APIResponse<IEnumerable<string>?>> List()
    {
        if (!HttpContext.OwnsTenantGame())
            return Unauthorized();

        return APIResponse(_diffProvider.List(basePath));
    }

    [HttpGet("TreeFlat")]
    [Produces("application/json")]
    public ActionResult<APIResponse<IEnumerable<string>?>> TreeFlat(string entry)
    {
        if (!HttpContext.OwnsTenantGame())
            return Unauthorized();

        return APIResponse(_diffProvider.TreeFlat(basePath, entry));
    }

    [HttpGet("Get")]
    [Produces("application/json")]
    public ActionResult<APIResponse<IEnumerable<string>?>> Get(string path, CancellationToken ct)
    {
        if (!HttpContext.OwnsTenantGame())
            return Unauthorized();

        return APIResponse(_diffProvider.Get(basePath, path, ct));
    }

    [HttpPost("Search")]
    [Produces("application/json")]
    public ActionResult<APIResponse<IEnumerable<string>?>> Search(TextSearchFiltering[] filters, CancellationToken ct)
    {
        if (!HttpContext.OwnsTenantGame())
            return Unauthorized();

        return APIResponse(_diffProvider.Search(basePath, filters, ct));
    }
}