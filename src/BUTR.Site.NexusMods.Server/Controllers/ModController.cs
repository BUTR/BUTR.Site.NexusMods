using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Services.Database;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers
{
    [ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
    public sealed class ModController : ControllerBase
    {
        public sealed record ModQuery(int ModId);
        
        public sealed record PaginatedQuery(int Page, int PageSize);

        public sealed record ManualLinkQuery(string ModId, int NexusModsId);
        public sealed record ManualUnlinkQuery(string ModId);

        public sealed record AllowModQuery(int UserId, string ModId);
        public sealed record DisallowModQuery(int UserId, string ModId);


        private readonly ILogger _logger;
        private readonly NexusModsAPIClient _nexusModsAPIClient;
        private readonly NexusModsModProvider _nexusModsMod;
        private readonly ModNexusModsManualLinkProvider _modNexusModsManualLink;
        private readonly UserAllowedModsProvider _userAllowedMods;

        public ModController(
            ILogger<ModController> logger,
            NexusModsAPIClient nexusModsAPIClient,
            NexusModsModProvider nexusModsMod,
            ModNexusModsManualLinkProvider modNexusModsManualLink,
            UserAllowedModsProvider userAllowedMods)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _nexusModsAPIClient = nexusModsAPIClient ?? throw new ArgumentNullException(nameof(nexusModsAPIClient));
            _nexusModsMod = nexusModsMod ?? throw new ArgumentNullException(nameof(nexusModsMod));
            _modNexusModsManualLink = modNexusModsManualLink ?? throw new ArgumentNullException(nameof(modNexusModsManualLink));
            _userAllowedMods = userAllowedMods ?? throw new ArgumentNullException(nameof(userAllowedMods));
        }


        [HttpGet("Get/{gameDomain}/{modId:int}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ModModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Get(string gameDomain, int modId)
        {
            var userId = HttpContext.GetUserId();
            var apiKey = HttpContext.GetAPIKey();

            if (await _nexusModsAPIClient.GetMod(gameDomain, modId, apiKey) is not { } modInfo)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Mod not found!"));

            if (userId != modInfo.User.MemberId)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("User does not have access to the mod!"));

            return StatusCode(StatusCodes.Status200OK, new ModModel(modInfo.Name, modInfo.ModId));
        }

        [HttpGet("Paginated")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagingResponse<ModModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Paginated([FromQuery] PaginatedQuery query, CancellationToken ct)
        {
            var page = query.Page;
            var pageSize = Math.Max(Math.Min(query.PageSize, 20), 5);

            var (userModsTotalCount, userMods) = await _nexusModsMod.GetPaginatedAsync(HttpContext.GetUserId(), (page - 1) * pageSize, pageSize, ct);

            var metadata = new PagingMetadata
            {
                PageSize = pageSize,
                CurrentPage = page,
                TotalCount = userModsTotalCount,
                TotalPages = (int) Math.Ceiling((double) userModsTotalCount / (double) pageSize),
            };

            return StatusCode(StatusCodes.Status200OK, new PagingResponse<ModModel>
            {
                Items = userMods.Select(m => new ModModel(m.Name, m.ModId)).ToAsyncEnumerable(),
                Metadata = metadata
            });
        }

        [HttpPost("Update")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Update([FromBody] ModQuery query)
        {
            var userId = HttpContext.GetUserId();
            var apiKey = HttpContext.GetAPIKey();

            if (await _nexusModsAPIClient.GetMod("mountandblade2bannerlord", query.ModId, apiKey) is not { } modInfo)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Mod not found!"));

            if (userId != modInfo.User.MemberId)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("User does not have access to the mod!"));

            if (await _nexusModsMod.FindAsync(query.ModId) is { } entry)
            {
                if (await _nexusModsMod.UpsertAsync(entry with { Name = modInfo.Name }) is not null)
                    return StatusCode(StatusCodes.Status200OK, new StandardResponse("Updated successful!"));
            }
            return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Mod is not linked!"));
        }


        [HttpGet("Link")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Link([FromQuery] ModQuery query)
        {
            var userId = HttpContext.GetUserId();
            var apiKey = HttpContext.GetAPIKey();

            if (await _nexusModsAPIClient.GetMod("mountandblade2bannerlord", query.ModId, apiKey) is not { } modInfo)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Mod not found!"));

            if (userId != modInfo.User.MemberId)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("User does not have access to the mod!"));

            if (await _nexusModsMod.FindAsync(query.ModId) is { } entry)
            {
                if (entry.UserIds.Contains(userId))
                    return StatusCode(StatusCodes.Status200OK, new StandardResponse("Already linked!"));
            }
            else
            {
                entry = new NexusModsModTableEntry
                {
                    Name = modInfo.Name,
                    ModId = modInfo.ModId,
                    UserIds = ImmutableArray.Create<int>(userId)
                };
            }

            if (await _nexusModsMod.UpsertAsync(entry with { UserIds = entry.UserIds.Add(userId) }) is not null)
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
            var userId = HttpContext.GetUserId();

            if (await _nexusModsMod.FindAsync(query.ModId) is not { } entry)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Mod not found!"));

            if (!entry.UserIds.Contains(userId))
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Mod is not linked to user!"));

            if (await _nexusModsMod.UpsertAsync(entry with { UserIds = entry.UserIds.Remove(userId) }) is not null)
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Unlinked successful!"));

            return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Failed to unlink!"));
        }


        [HttpGet("ManualLink")]
        [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> ManualLink([FromQuery] ManualLinkQuery query)
        {
            if (await _modNexusModsManualLink.FindAsync(query.ModId) is not { } entry)
                entry = new ModNexusModsManualLinkTableEntry { ModId = query.ModId, NexusModsId = 0 };
            
            if (await _modNexusModsManualLink.UpsertAsync(entry with { NexusModsId = query.NexusModsId }) is not null)
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Linked successful!"));

            return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Failed to link!"));
        }

        [HttpGet("ManualUnlink")]
        [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> ManualUnlink([FromQuery] ManualUnlinkQuery query)
        {
            if (await _modNexusModsManualLink.FindAsync(query.ModId) is null)
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Mod Id was not linked!"));
            
            if (await _modNexusModsManualLink.UpsertAsync(new ModNexusModsManualLinkTableEntry { ModId = query.ModId, NexusModsId = 0 }) is not null)
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Unlinked successful!"));

            return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Failed to unlink!"));
        }
        
        [HttpGet("ManualLinkPaginated")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagingResponse<ModNexusModsManualLinkModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> ManualLinkPaginated([FromQuery] PaginatedQuery query, CancellationToken ct)
        {
            var page = query.Page;
            var pageSize = Math.Max(Math.Min(query.PageSize, 20), 5);

            var (count, entries) = await _modNexusModsManualLink.GetPaginatedAsync((page - 1) * pageSize, pageSize, ct);

            var metadata = new PagingMetadata
            {
                PageSize = pageSize,
                CurrentPage = page,
                TotalCount = count,
                TotalPages = (int) Math.Ceiling((double) count / (double) pageSize),
            };

            return StatusCode(StatusCodes.Status200OK, new PagingResponse<ModNexusModsManualLinkModel>
            {
                Items = entries.Select(m => new ModNexusModsManualLinkModel(m.ModId, m.NexusModsId)).ToAsyncEnumerable(),
                Metadata = metadata
            });
        }


        [HttpGet("AllowMod")]
        [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> AllowMod([FromQuery] AllowModQuery query)
        {
            if (await _userAllowedMods.FindAsync(query.UserId) is not { } entry)
                entry = new UserAllowedModsTableEntry { UserId = query.UserId, AllowedModIds = ImmutableArray.Create<string>() };

            if (entry.AllowedModIds.Contains(query.ModId))
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Mod Id is already allowed!"));

            if (await _userAllowedMods.UpsertAsync(entry with { AllowedModIds = entry.AllowedModIds.Add(query.ModId) }) is not null)
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Allowed successful!"));

            return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Failed to allowed!"));
        }

        [HttpGet("DisallowMod")]
        [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> DisallowMod([FromQuery] DisallowModQuery query)
        {
            if (await _userAllowedMods.FindAsync(query.UserId) is not { } entry)
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Mod Id was not allowed!"));

            if (!entry.AllowedModIds.Contains(query.ModId))
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Mod Id is already disallowed!"));

            if (await _userAllowedMods.UpsertAsync(entry with { AllowedModIds = entry.AllowedModIds.Remove(query.ModId) }) is not null)
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Disallowed successful!"));

            return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Failed to disallowed!"));
        }
        
        [HttpGet("AllowModPaginated")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagingResponse<UserAllowedModsModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> AllowModPaginated([FromQuery] PaginatedQuery query, CancellationToken ct)
        {
            var page = query.Page;
            var pageSize = Math.Max(Math.Min(query.PageSize, 20), 5);

            var (count, entries) = await _userAllowedMods.GetPaginatedAsync((page - 1) * pageSize, pageSize, ct);

            var metadata = new PagingMetadata
            {
                PageSize = pageSize,
                CurrentPage = page,
                TotalCount = count,
                TotalPages = (int) Math.Ceiling((double) count / (double) pageSize),
            };

            return StatusCode(StatusCodes.Status200OK, new PagingResponse<UserAllowedModsModel>
            {
                Items = entries.Select(m => new UserAllowedModsModel(m.UserId, m.AllowedModIds)).ToAsyncEnumerable(),
                Metadata = metadata
            });
        }
    }
}