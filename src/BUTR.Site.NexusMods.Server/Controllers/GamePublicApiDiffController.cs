using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.APIResponses;
using BUTR.Site.NexusMods.Server.Utils.BindingSources;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), TenantRequired]
public sealed class GamePublicApiDiffController : ControllerExtended
{
    private const string basePath = "/public-api-diff";

    private readonly ILogger _logger;
    private readonly DiffProvider _diffProvider;

    public GamePublicApiDiffController(ILogger<GamePublicApiDiffController> logger, DiffProvider diffProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _diffProvider = diffProvider ?? throw new ArgumentNullException(nameof(diffProvider));
    }

    [HttpGet("List")]
    [Produces("application/json")]
    public APIResponseActionResult<IEnumerable<string>?> List() => APIResponse(_diffProvider.List(basePath));

    [HttpGet("TreeFlat")]
    [Produces("application/json")]
    public APIResponseActionResult<IEnumerable<string>?> TreeFlat(string entry) => APIResponse(_diffProvider.TreeFlat(basePath, entry));

    [HttpGet("Get")]
    [Produces("application/json")]
    public APIResponseActionResult<IEnumerable<string>?> Get(string path, CancellationToken ct) => APIResponse(_diffProvider.Get(basePath, path, ct));

    [HttpPost("Search")]
    [Produces("application/json")]
    public APIResponseActionResult<IEnumerable<string>?> Search(TextSearchFiltering[] filters, CancellationToken ct) => APIResponse(_diffProvider.Search(basePath, filters, ct));
}