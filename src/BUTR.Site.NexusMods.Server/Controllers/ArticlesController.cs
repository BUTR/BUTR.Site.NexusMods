using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers
{
    [ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
    public class ArticlesController : ControllerBase
    {
        public sealed record PaginatedQuery(uint Page, uint PageSize);

        private readonly ILogger _logger;
        private readonly AppDbContext _dbContext;

        public ArticlesController(ILogger<ArticlesController> logger, AppDbContext dbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        [HttpGet("Paginated")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagingResponse<ArticleModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> GetAll([FromQuery] PaginatedQuery query, CancellationToken ct)
        {
            var page = query.Page;
            var pageSize = Math.Max(Math.Min(query.PageSize, 100), 5);

            var dbQuery = _dbContext.Set<NexusModsArticleEntity>()
                .OrderBy(x => x.ArticleId);
            var paginated = await dbQuery.PaginatedAsync(page, pageSize, ct);

            return StatusCode(StatusCodes.Status200OK, new PagingResponse<ArticleModel>
            {
                Items = paginated.Items.Select(x => new ArticleModel(x.ArticleId, x.Title, x.AuthorId, x.AuthorName, x.CreateDate)).ToAsyncEnumerable(),
                Metadata = paginated.Metadata
            });
        }
    }
}