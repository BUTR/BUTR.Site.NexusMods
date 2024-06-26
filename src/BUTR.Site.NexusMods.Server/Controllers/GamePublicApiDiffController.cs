using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.Threading;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), TenantRequired]
public sealed class GamePublicApiDiffController : ApiControllerBase
{
    private const string basePath = "/public-api-diff";

    private readonly ILogger _logger;
    private readonly IDiffProvider _diffProvider;

    public GamePublicApiDiffController(ILogger<GamePublicApiDiffController> logger, IDiffProvider diffProvider)
    {
        _logger = logger;
        _diffProvider = diffProvider;
    }

    [HttpGet("List")]
    public ApiResult<IEnumerable<string>?> List() => ApiResult(_diffProvider.List(basePath));

    [HttpGet("TreeFlat")]
    public ApiResult<IEnumerable<string>?> TreeFlat(string entry) => ApiResult(_diffProvider.TreeFlat(basePath, entry));

    [HttpGet("Get")]
    public ApiResult<IEnumerable<string>?> Get(string path, CancellationToken ct) => ApiResult(_diffProvider.Get(basePath, path, ct));

    [HttpPost("Search")]
    public ApiResult<IEnumerable<string>?> Search(TextSearchFiltering[] filters, CancellationToken ct) => ApiResult(_diffProvider.Search(basePath, filters, ct));
}