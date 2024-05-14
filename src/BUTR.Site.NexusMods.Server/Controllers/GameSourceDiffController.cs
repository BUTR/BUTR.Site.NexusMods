using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), ButrNexusModsAuthorization, TenantRequired]
public sealed class GameSourceDiffController : ApiControllerBase
{
    private const string basePath = "/source-api-diff";

    private readonly ILogger _logger;
    private readonly IDiffProvider _diffProvider;

    public GameSourceDiffController(ILogger<GameSourceDiffController> logger, IDiffProvider diffProvider)
    {
        _logger = logger;
        _diffProvider = diffProvider;
    }

    [HttpGet("List")]
    public ApiResult<IEnumerable<string>?> List()
    {
        if (!HttpContext.OwnsTenantGame())
            return ApiResultError("Game not owned!", StatusCodes.Status401Unauthorized);

        return ApiResult(_diffProvider.List(basePath));
    }

    [HttpGet("TreeFlat")]
    public ApiResult<IEnumerable<string>?> TreeFlat(string entry)
    {
        if (!HttpContext.OwnsTenantGame())
            return ApiResultError("Game not owned!", StatusCodes.Status401Unauthorized);

        return ApiResult(_diffProvider.TreeFlat(basePath, entry));
    }

    [HttpGet("Get")]
    public ApiResult<IEnumerable<string>?> Get(string path, CancellationToken ct)
    {
        if (!HttpContext.OwnsTenantGame())
            return ApiResultError("Game not owned!", StatusCodes.Status401Unauthorized);

        return ApiResult(_diffProvider.Get(basePath, path, ct));
    }

    [HttpPost("Search")]
    public ApiResult<IEnumerable<string>?> Search(TextSearchFiltering[] filters, CancellationToken ct)
    {
        if (!HttpContext.OwnsTenantGame())
            return ApiResultError("Game not owned!", StatusCodes.Status401Unauthorized);

        return ApiResult(_diffProvider.Search(basePath, filters, ct));
    }
}