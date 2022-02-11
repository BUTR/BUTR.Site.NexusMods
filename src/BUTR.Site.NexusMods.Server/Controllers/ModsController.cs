using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Services.Database;
using BUTR.Site.NexusMods.Shared.Helpers;
using BUTR.Site.NexusMods.Shared.Models;
using BUTR.Site.NexusMods.Shared.Models.API;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers
{
    [ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
    public class ModsController : ControllerBase
    {
        public record ModQuery(string GameDomain, int ModId);
        public record ModsQuery(int Page, int PageSize);

        private readonly ILogger _logger;
        private readonly NexusModsAPIClient _nexusModsAPIClient;
        private readonly ModsProvider _sqlHelperMods;

        public ModsController(ILogger<ModsController> logger, NexusModsAPIClient nexusModsAPIClient, ModsProvider sqlHelperMods)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _nexusModsAPIClient = nexusModsAPIClient ?? throw new ArgumentNullException(nameof(nexusModsAPIClient));
            _sqlHelperMods = sqlHelperMods ?? throw new ArgumentNullException(nameof(sqlHelperMods));
        }

        [HttpGet("{gameDomain}/{modId:int}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ModModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Get(string gameDomain, int modId)
        {
            if (User.IsInRole(ApplicationRoles.Administrator) || User.IsInRole(ApplicationRoles.Moderator))
                return StatusCode(StatusCodes.Status200OK, new ModModel("", "", 0));

            var userId = HttpContext.GetUserId();
            var apiKey = HttpContext.GetAPIKey();

            if (await _nexusModsAPIClient.GetMod(gameDomain, modId, apiKey) is not { } modInfo)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Mod not found!"));

            if (userId != modInfo.User.MemberId)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("User does not have access to the mod!"));

            return StatusCode(StatusCodes.Status200OK, new ModModel(modInfo.Name, modInfo.DomainName, modInfo.ModId));
        }

        [HttpGet("")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagingResponse<ModModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Paginated([FromQuery] ModsQuery query, CancellationToken ct)
        {
            var page = query.Page;
            var pageSize = Math.Max(Math.Min(query.PageSize, 20), 5);

            if (User.IsInRole(ApplicationRoles.Administrator) || User.IsInRole(ApplicationRoles.Moderator))
            {
                return StatusCode(StatusCodes.Status200OK, new PagingResponse<ModModel>
                {
                    Items = AsyncEnumerable.Empty<ModModel>(),
                    Metadata = new PagingMetadata()
                });
            }

            var (userModsTotalCount, userMods) = await _sqlHelperMods.GetPaginatedAsync(HttpContext.GetUserId(), (page - 1) * pageSize, pageSize, ct);

            var metadata = new PagingMetadata
            {
                PageSize = pageSize,
                CurrentPage = page,
                TotalCount = userModsTotalCount,
                TotalPages = (int) Math.Ceiling((double) userModsTotalCount / (double) pageSize),
            };

            return StatusCode(StatusCodes.Status200OK, new PagingResponse<ModModel>
            {
                Items = userMods.Select(m => new ModModel(m.Name, m.GameDomain, m.ModId)).ToAsyncEnumerable(),
                Metadata = metadata
            });
        }

        [HttpGet("Link")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Link([FromQuery] ModQuery query)
        {
            if (User.IsInRole(ApplicationRoles.Administrator) || User.IsInRole(ApplicationRoles.Moderator))
                return StatusCode(StatusCodes.Status200OK, new StandardResponse(""));

            var userId = HttpContext.GetUserId();
            var apiKey = HttpContext.GetAPIKey();

            if (await _nexusModsAPIClient.GetMod(query.GameDomain, query.ModId, apiKey) is not { } modInfo)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Mod not found!"));

            if (userId != modInfo.User.MemberId)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("User does not have access to the mod!"));

            if (await _sqlHelperMods.FindAsync(query.GameDomain, query.ModId) is { } mod)
            {
                if (mod.UserIds.Contains(userId))
                    return StatusCode(StatusCodes.Status200OK, new StandardResponse("Already linked!"));
            }
            else
            {
                mod = new ModTableEntry
                {
                    Name = modInfo.Name,
                    GameDomain = modInfo.DomainName,
                    ModId = modInfo.ModId,
                    UserIds = new[] { userId }
                };
            }

            if (await _sqlHelperMods.UpsertAsync(mod with { UserIds = mod.UserIds.Concat(new[] { userId }).ToArray() }) is not null)
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Linked successful!"));

            return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Failed to link!"));
        }

        [HttpGet("Unlink")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Unlink([FromQuery] ModQuery query)
        {
            if (User.IsInRole(ApplicationRoles.Administrator) || User.IsInRole(ApplicationRoles.Moderator))
                return StatusCode(StatusCodes.Status200OK, new StandardResponse(""));

            var userId = HttpContext.GetUserId();

            if (await _sqlHelperMods.FindAsync(query.GameDomain, query.ModId) is not { } mod)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Mod not found!"));

            if (!mod.UserIds.Contains(userId))
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Mod is not linked to user!"));

            if (await _sqlHelperMods.UpsertAsync(mod with { UserIds = mod.UserIds.Where(uid => uid != userId).ToArray() }) is not null)
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Unlinked successful!"));

            return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Failed to unlink!"));
        }

        [HttpGet("Refresh")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Refresh([FromQuery] ModQuery query)
        {
            if (User.IsInRole(ApplicationRoles.Administrator) || User.IsInRole(ApplicationRoles.Moderator))
                return StatusCode(StatusCodes.Status200OK, new StandardResponse(""));

            var userId = HttpContext.GetUserId();
            var apiKey = HttpContext.GetAPIKey();

            if (await _nexusModsAPIClient.GetMod(query.GameDomain, query.ModId, apiKey) is not { } modInfo)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Mod not found!"));

            if (userId != modInfo.User.MemberId)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("User does not have access to the mod!"));

            if (await _sqlHelperMods.FindAsync(query.GameDomain, query.ModId) is { } mod)
            {
                if (await _sqlHelperMods.UpsertAsync(mod with { Name = modInfo.Name }) is not null)
                    return StatusCode(StatusCodes.Status200OK, new StandardResponse("Updated successful!"));
            }
            return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Mod is not linked!"));
        }
    }
}