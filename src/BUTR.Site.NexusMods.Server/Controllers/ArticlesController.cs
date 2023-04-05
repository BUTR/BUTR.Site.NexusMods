using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
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
        [ProducesResponseType(typeof(APIResponse<PagingData<ArticleModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse<PagingData<ArticleModel>?>>> Paginated([FromBody] PaginatedQuery query, CancellationToken ct)
        {
            var paginated = await _dbContext.Set<NexusModsArticleEntity>()
                .PaginatedAsync(query, 100, new() { Property = nameof(NexusModsArticleEntity.ArticleId), Type = SortingType.Ascending }, ct);

            return Result(APIResponse.From(new PagingData<ArticleModel>
            {
                Items = paginated.Items.Select(x => new ArticleModel(x.ArticleId, x.Title, x.AuthorId, x.AuthorName, x.CreateDate)).ToAsyncEnumerable(),
                Metadata = paginated.Metadata
            }));
        }

        [HttpGet("Autocomplete")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(APIResponse<IQueryable<string>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public ActionResult<APIResponse<IQueryable<string>?>> Autocomplete([FromQuery] string authorName)
        {
            return Result(APIResponse.From(_dbContext.AutocompleteStartsWith<NexusModsArticleEntity, string>(x => x.AuthorName, authorName)));
        }
    }
}