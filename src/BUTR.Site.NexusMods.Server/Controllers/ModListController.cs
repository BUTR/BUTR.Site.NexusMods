using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Services.Database;
using BUTR.Site.NexusMods.Shared.Models;
using BUTR.Site.NexusMods.Shared.Models.API;

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
    public class ModListController : ControllerBase
    {
        public record ModListCreateQuery(string Name, string Content);
        public record ModListDeleteQuery(string Name);
        public record ModListQuery(string? Name, bool? ShowUserOwn, int Page, int PageSize);

        private readonly ILogger _logger;
        private readonly ModListProvider _modListProvider;

        public ModListController(ILogger<ModListController> logger, ModListProvider modListProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _modListProvider = modListProvider ?? throw new ArgumentNullException(nameof(modListProvider));
        }

        [HttpPost("")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Create([FromBody] ModListCreateQuery query)
        {
            var newId = Guid.NewGuid();
            if (await _modListProvider.UpsertAsync(new() { Id = newId, UserId = HttpContext.GetUserId(), Name = query.Name, Content = query.Content }) is { } mod)
            {
                if (mod.Id != newId)
                    return StatusCode(StatusCodes.Status200OK, new StandardResponse("Updated!"));
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Created!"));
            }

            return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Failed to create!"));
        }

        [HttpDelete("")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Delete([FromBody] ModListDeleteQuery query)
        {
            if (await _modListProvider.FindAsync(query.Name, HttpContext.GetUserId()) is not { } mod)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Not found!"));

            if (await _modListProvider.DeleteAsync(mod))
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Deleted successful!"));

            return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Failed to deleted!"));
        }

        [HttpGet("List")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagingResponse<ModListModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> List([FromQuery] ModListQuery query)
        {
            var page = query.Page;
            var pageSize = Math.Max(Math.Min(query.PageSize, 20), 5);

            if (query.ShowUserOwn is { } val && val)
            {
                return await ForUser(query.Name ?? string.Empty, HttpContext.GetUserId(), page, pageSize);
            }

            return await ForAll(query.Name ?? string.Empty, page, pageSize);
        }

        private async Task<ObjectResult> ForAll(string name, int page, int pageSize)
        {
            var (modListsCount, modLists) = await _modListProvider.GetPaginatedAsync(-1, name, (page - 1) * pageSize, pageSize, CancellationToken.None);

            var metadata = new PagingMetadata
            {
                PageSize = pageSize,
                CurrentPage = page,
                TotalCount = modListsCount,
                TotalPages = (int) Math.Floor((double) modListsCount / (double) pageSize),
            };

            return StatusCode(StatusCodes.Status200OK, new PagingResponse<ModListModel>
            {
                Items = modLists.Select(m => new ModListModel(m.Name, m.Content)).ToAsyncEnumerable(),
                Metadata = metadata
            });
        }
        private async Task<ObjectResult> ForUser(string name, int userId, int page, int pageSize)
        {
            var (modListsCount, modLists) = await _modListProvider.GetPaginatedAsync(userId, name, (page - 1) * pageSize, pageSize, CancellationToken.None);

            var metadata = new PagingMetadata
            {
                PageSize = pageSize,
                CurrentPage = page,
                TotalCount = modListsCount,
                TotalPages = (int) Math.Floor((double) modListsCount / (double) pageSize),
            };

            return StatusCode(StatusCodes.Status200OK, new PagingResponse<ModListModel>
            {
                Items = modLists.Select(m => new ModListModel(m.Name, m.Content)).ToAsyncEnumerable(),
                Metadata = metadata
            });
        }
    }
}