using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Services;
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

        public sealed record PaginatedQuery(uint Page, uint PageSize);

        public sealed record ManualLinkQuery(string ModId, int NexusModsId);
        public sealed record ManualUnlinkQuery(string ModId);

        public sealed record AllowModQuery(int UserId, string ModId);
        public sealed record DisallowModQuery(int UserId, string ModId);


        private readonly ILogger _logger;
        private readonly NexusModsAPIClient _nexusModsAPIClient;
        private readonly AppDbContext _dbContext;
        private readonly NexusModsInfo _nexusModsInfo;

        public ModController(ILogger<ModController> logger, NexusModsAPIClient nexusModsAPIClient, AppDbContext dbContext, NexusModsInfo nexusModsInfo)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _nexusModsAPIClient = nexusModsAPIClient ?? throw new ArgumentNullException(nameof(nexusModsAPIClient));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _nexusModsInfo = nexusModsInfo ?? throw new ArgumentNullException(nameof(nexusModsInfo));
        }


        [HttpGet("Get/{gameDomain}/{modId:int}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ModModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Get(string gameDomain, int modId, CancellationToken ct)
        {
            var userId = HttpContext.GetUserId();
            var apiKey = HttpContext.GetAPIKey();

            if (await _nexusModsAPIClient.GetModAsync(gameDomain, modId, apiKey) is not { } modInfo)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Mod not found!"));

            if (userId != modInfo.User.Id)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("User does not have access to the mod!"));

            /*
            var allowedUserIds = await _dbContext.Set<NexusModsModEntity>()
                .Where(x => x.NexusModsModId == modInfo.Id)
                .SelectMany(x => _dbContext.Set<UserAllowedModsEntity>(), (x, y) => new {x, y})
                .Join(_dbContext.Set<ModNexusModsManualLinkEntity>(), x => x.x.NexusModsModId, z => z.NexusModsId, (x, z) => new {x, z})
                .Where(x => x.x.x.UserIds.Contains(userId))
                .Where(x => x.x.y.AllowedModIds.Contains(x.z.ModId))
                .Select(x => x.x.y.UserId)
                .ToImmutableArrayAsync(ct);
            */

            return StatusCode(StatusCodes.Status200OK, new ModModel(modInfo.Name, modInfo.Id, ImmutableArray<int>.Empty));
        }

        [HttpGet("Paginated")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagingResponse<ModModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Paginated([FromQuery] PaginatedQuery query, CancellationToken ct)
        {
            var page = query.Page;
            var pageSize = Math.Max(Math.Min(query.PageSize, 20), 5);

            var userId = HttpContext.GetUserId();

            var dbQuery = _dbContext.Set<NexusModsModEntity>()
                .Where(y => y.UserIds.Contains(userId))
                .OrderBy(x => x.NexusModsModId);
            var paginated = await dbQuery.PaginatedAsync(page, pageSize, ct);

            /*
            var allowedUserIds = await _dbContext.Set<NexusModsModEntity>()
                .SelectMany(x => _dbContext.Set<UserAllowedModsEntity>(), (x, y) => new {x, y})
                .Join(_dbContext.Set<ModNexusModsManualLinkEntity>(), x => x.x.NexusModsModId, z => z.NexusModsId, (x, z) => new {x, z})
                .Where(x => x.x.x.UserIds.Contains(userId))
                .Where(x => x.x.y.AllowedModIds.Contains(x.z.ModId))
                .Select(x => x.x.y.UserId)
                .ToImmutableArrayAsync(ct);
            */

            return StatusCode(StatusCodes.Status200OK, new PagingResponse<ModModel>
            {
                Items = paginated.Items.Select(m => new ModModel(m.Name, m.NexusModsModId, ImmutableArray<int>.Empty)).ToAsyncEnumerable(),
                Metadata = paginated.Metadata
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

            if (await _nexusModsAPIClient.GetModAsync("mountandblade2bannerlord", query.ModId, apiKey) is not { } modInfo)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Mod not found!"));

            if (userId != modInfo.User.Id)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("User does not have access to the mod!"));

            NexusModsModEntity? ApplyChanges(NexusModsModEntity? existing) => existing switch
            {
                null => null,
                var entity => entity with { Name = modInfo.Name }
            };
            if (await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsModEntity>(x => x.NexusModsModId == query.ModId, ApplyChanges))
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Updated successful!"));

            return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Failed to link the mod!"));
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

            if (await _nexusModsAPIClient.GetModAsync("mountandblade2bannerlord", query.ModId, apiKey) is not { } modInfo)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Mod not found!"));

            if (userId != modInfo.User.Id)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("User does not have access to the mod!"));

            if (HttpContext.GetIsPremium())
            {
                var exposedModIds = await _nexusModsInfo.GetModIdsAsync("mountandblade2bannerlord", modInfo.Id, apiKey).Distinct().ToImmutableArrayAsync();
                NexusModsExposedModsEntity? ApplyChanges2(NexusModsExposedModsEntity? existing) => existing switch
                {
                    null => new() { NexusModsModId = modInfo.Id, ModIds = exposedModIds.AsArray(), LastCheckedDate = DateTime.UtcNow },
                    var entity => entity with { ModIds = entity.ModIds.AsImmutableArray().AddRange(exposedModIds.Except(entity.ModIds)).AsArray(), LastCheckedDate = DateTime.UtcNow }
                };
                if (!await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsExposedModsEntity>(x => x.NexusModsModId == query.ModId, ApplyChanges2))
                    return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Failed to link!"));
            }

            NexusModsModEntity? ApplyChanges(NexusModsModEntity? existing) => existing switch
            {
                null => new() { Name = modInfo.Name, NexusModsModId = modInfo.Id, UserIds = ImmutableArray.Create<int>(userId).AsArray() },
                var entity when entity.UserIds.Contains(userId) => entity,
                var entity => entity with { UserIds = ImmutableArray.Create<int>(userId).AsArray() }
            };
            if (await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsModEntity>(x => x.NexusModsModId == query.ModId, ApplyChanges))
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

            NexusModsModEntity? ApplyChanges(NexusModsModEntity? existing) => existing switch
            {
                null => null,
                var entity when entity.UserIds.Contains(userId) && entity.UserIds.Length == 1 => null,
                var entity when !entity.UserIds.Contains(userId) => entity,
                var entity => entity with { UserIds = entity.UserIds.AsImmutableArray().Remove(userId).AsArray() }
            };
            if (await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsModEntity>(x => x.NexusModsModId == query.ModId, ApplyChanges))
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
            ModNexusModsManualLinkEntity? ApplyChanges(ModNexusModsManualLinkEntity? existing) => existing switch
            {
                null => new() { ModId = query.ModId, NexusModsId = query.NexusModsId },
                var entity => entity with { NexusModsId = query.NexusModsId }
            };
            if (await _dbContext.AddUpdateRemoveAndSaveAsync<ModNexusModsManualLinkEntity>(x => x.ModId == query.ModId, ApplyChanges))
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
            ModNexusModsManualLinkEntity? ApplyChanges(ModNexusModsManualLinkEntity? existing) => existing switch
            {
                _ => null
            };
            if (await _dbContext.AddUpdateRemoveAndSaveAsync<ModNexusModsManualLinkEntity>(x => x.ModId == query.ModId, ApplyChanges))
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

            var dbQuery = _dbContext.Set<ModNexusModsManualLinkEntity>()
                .OrderBy(y => y.ModId);

            var paginated = await dbQuery.PaginatedAsync(page, pageSize, ct);

            return StatusCode(StatusCodes.Status200OK, new PagingResponse<ModNexusModsManualLinkModel>
            {
                Items = paginated.Items.Select(m => new ModNexusModsManualLinkModel(m.ModId, m.NexusModsId)).ToAsyncEnumerable(),
                Metadata = paginated.Metadata
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
            UserAllowedModsEntity? ApplyChanges(UserAllowedModsEntity? existing) => existing switch
            {
                null => new() { UserId = query.UserId, AllowedModIds = ImmutableArray.Create<string>(query.ModId).AsArray() },
                var entity when entity.AllowedModIds.Contains(query.ModId) => entity,
                var entity => entity with { AllowedModIds = entity.AllowedModIds.AsImmutableArray().Add(query.ModId).AsArray() }
            };
            if (await _dbContext.AddUpdateRemoveAndSaveAsync<UserAllowedModsEntity>(x => x.UserId == query.UserId, ApplyChanges))
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
            UserAllowedModsEntity? ApplyChanges(UserAllowedModsEntity? existing) => existing switch
            {
                null => null,
                var entity when entity.AllowedModIds.Contains(query.ModId) && entity.AllowedModIds.Length == 1 => null,
                var entity when !entity.AllowedModIds.Contains(query.ModId) => entity,
                var entity => entity with { AllowedModIds = entity.AllowedModIds.AsImmutableArray().Remove(query.ModId).AsArray() }
            };
            if (await _dbContext.AddUpdateRemoveAndSaveAsync<UserAllowedModsEntity>(x => x.UserId == query.UserId, ApplyChanges))
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

            var dbQuery = _dbContext.Set<UserAllowedModsEntity>()
                .OrderBy(y => y.UserId);

            var paginated = await dbQuery.PaginatedAsync(page, pageSize, ct);

            return StatusCode(StatusCodes.Status200OK, new PagingResponse<UserAllowedModsModel>
            {
                Items = paginated.Items.Select(m => new UserAllowedModsModel(m.UserId, m.AllowedModIds.AsImmutableArray())).ToAsyncEnumerable(),
                Metadata = paginated.Metadata
            });
        }
    }
}