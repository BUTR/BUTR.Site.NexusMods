using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
public class ArticlesController : ControllerExtended
{
    private readonly ILogger _logger;
    private readonly AppDbContext _dbContext;

    public ArticlesController(ILogger<ArticlesController> logger, AppDbContext dbContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    [HttpPost("Paginated")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<PagingData<ArticleModel>?>>> PaginatedAsync([FromBody] PaginatedQuery query, CancellationToken ct)
    {
        var paginated = await _dbContext.Set<NexusModsArticleEntity>()
            .PaginatedAsync(query, 100, new() { Property = nameof(NexusModsArticleEntity.NexusModsArticleId), Type = SortingType.Ascending }, ct);

        return APIPagingResponse(paginated, items => items.Select(x => new ArticleModel(x.NexusModsArticleId, x.Title, x.NexusModsUserId, x.AuthorName, x.CreateDate)));
    }

    [HttpGet("Autocomplete")]
    [Produces("application/json")]
    public ActionResult<APIResponse<IQueryable<string>?>> Autocomplete([FromQuery] string authorName)
    {
        return APIResponse(_dbContext.AutocompleteStartsWith<NexusModsArticleEntity, string>(x => x.AuthorName, authorName));
    }
}