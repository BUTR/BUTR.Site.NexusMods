using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.APIResponses;
using BUTR.Site.NexusMods.Server.Utils.BindingSources;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), ButrNexusModsAuthorization, TenantRequired]
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
    public APIResponseActionResult<IEnumerable<string>?> List()
    {
        if (!HttpContext.OwnsTenantGame())
            return APIResponseError<IEnumerable<string>?>(StatusCodes.Status401Unauthorized);

        return APIResponse(_diffProvider.List(basePath));
    }

    [HttpGet("TreeFlat")]
    [Produces("application/json")]
    public APIResponseActionResult<IEnumerable<string>?> TreeFlat(string entry)
    {
        if (!HttpContext.OwnsTenantGame())
            return APIResponseError<IEnumerable<string>?>(StatusCodes.Status401Unauthorized);

        return APIResponse(_diffProvider.TreeFlat(basePath, entry));
    }

    [HttpGet("Get")]
    [Produces("application/json")]
    public APIResponseActionResult<IEnumerable<string>?> Get(string path, CancellationToken ct)
    {
        if (!HttpContext.OwnsTenantGame())
            return APIResponseError<IEnumerable<string>?>(StatusCodes.Status401Unauthorized);

        return APIResponse(_diffProvider.Get(basePath, path, ct));
    }

    [HttpPost("Search")]
    [Produces("application/json")]
    public APIResponseActionResult<IEnumerable<string>?> Search(TextSearchFiltering[] filters, CancellationToken ct)
    {
        if (!HttpContext.OwnsTenantGame())
            return APIResponseError<IEnumerable<string>?>(StatusCodes.Status401Unauthorized);

        return APIResponse(_diffProvider.Search(basePath, filters, ct));
    }
}