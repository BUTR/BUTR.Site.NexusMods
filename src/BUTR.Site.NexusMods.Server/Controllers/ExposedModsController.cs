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
    public class ExposedModsController : ControllerExtended
    {
        private readonly ILogger _logger;
        private readonly AppDbContext _dbContext;

        public ExposedModsController(ILogger<ExposedModsController> logger, AppDbContext dbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        [HttpPost("Paginated")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(APIResponse<PagingData<ExposedModModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse<PagingData<ExposedModModel>?>>> Paginated([FromBody] PaginatedQuery query, CancellationToken ct)
        {
            var paginated = await _dbContext.Set<NexusModsExposedModsEntity>()
                .Where(x => x.ModIds.Length > 0)
                .PaginatedAsync(query, 100, new() { Property = nameof(NexusModsExposedModsEntity.NexusModsModId), Type = SortingType.Ascending }, ct);

            return Result(APIResponse.From(new PagingData<ExposedModModel>
            {
                Items = paginated.Items.Select(x => new ExposedModModel(x.NexusModsModId, x.ModIds, x.LastCheckedDate)).ToAsyncEnumerable(),
                Metadata = paginated.Metadata
            }));
        }

        [HttpGet("Autocomplete")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(APIResponse<IQueryable<string>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public ActionResult<APIResponse<IQueryable<string>?>> Autocomplete([FromQuery] string modId)
        {
            return Result(APIResponse.From(_dbContext.AutocompleteStartsWith<NexusModsExposedModsEntity, string[]>(x => x.ModIds, modId)));
        }
    }
}