using BUTR.CrashReportViewer.Server.Helpers;
using BUTR.CrashReportViewer.Server.Models.Database;
using BUTR.CrashReportViewer.Shared.Models;
using BUTR.NexusMods.Server.Core.Helpers;
using BUTR.NexusMods.Shared.Helpers;
using BUTR.NexusMods.Shared.Models.API;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.CrashReportViewer.Server.Controllers
{
    [ApiController, Route("[controller]"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ModsController : ControllerBase
    {
        public record LinkModQuery(string GameDomain, int ModId);
        public record ModsQuery(int Page, int PageSize);

        private readonly ILogger _logger;
        private readonly NexusModsAPIClient _nexusModsAPIClient;
        private readonly SqlHelperMods _sqlHelperMods;

        public ModsController(ILogger<ModsController> logger, NexusModsAPIClient nexusModsAPIClient, SqlHelperMods sqlHelperMods)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _nexusModsAPIClient = nexusModsAPIClient ?? throw new ArgumentNullException(nameof(nexusModsAPIClient));
            _sqlHelperMods = sqlHelperMods ?? throw new ArgumentNullException(nameof(sqlHelperMods));
        }

        [HttpGet("")]
        public async Task<ActionResult> GetAll([FromQuery] ModsQuery query)
        {
            var page = query.Page;
            var pageSize = Math.Max(Math.Min(query.PageSize, 20), 5);

            if (User.IsInRole(ApplicationRoles.Administrator) || User.IsInRole(ApplicationRoles.Moderator))
            {
                return StatusCode((int) HttpStatusCode.OK, new PagingResponse<ModModel>
                {
                    Items = new List<ModModel>(),
                    Metadata = new PagingMetadata()
                });
            }

            if (!HttpContext.User.HasClaim(c => c.Type == "nmapikey") || HttpContext.User.Claims.FirstOrDefault(c => c.Type == "nmapikey") is not { } apiKeyClaim)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Invalid Bearer!"));

            if (await _nexusModsAPIClient.ValidateAPIKey(apiKeyClaim.Value) is not { } validateResponse)
                return StatusCode((int) HttpStatusCode.Unauthorized, new StandardResponse("Invalid NexusMods API Key from Bearer!"));

            var (userModsTotalCount, userMods) = await _sqlHelperMods.GetAsync(validateResponse.UserId, (page - 1) * pageSize, pageSize, CancellationToken.None);

            var metadata = new PagingMetadata
            {
                PageSize = pageSize,
                CurrentPage = page,
                TotalCount = userModsTotalCount,
                TotalPages = (int) Math.Ceiling((double) userModsTotalCount / (double) pageSize),
            };

            return StatusCode((int) HttpStatusCode.OK, new PagingResponse<ModModel>
            {
                Items = userMods.Select(m => new ModModel(m.Name, m.GameDomain, m.ModId)),
                Metadata = metadata
            });
        }

        [HttpGet("Get")]
        public async Task<ActionResult> Get([FromQuery] LinkModQuery query)
        {
            if (User.IsInRole(ApplicationRoles.Administrator) || User.IsInRole(ApplicationRoles.Moderator))
                return StatusCode((int) HttpStatusCode.OK, new ModModel("", "", 0));

            if (!HttpContext.User.HasClaim(c => c.Type == "nmapikey") || HttpContext.User.Claims.FirstOrDefault(c => c.Type == "nmapikey") is not { } apiKeyClaim)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Invalid Bearer!"));

            if (await _nexusModsAPIClient.ValidateAPIKey(apiKeyClaim.Value) is not { } validateResponse)
                return StatusCode((int) HttpStatusCode.Unauthorized, new StandardResponse("Invalid NexusMods API Key from Bearer!"));

            if (await _nexusModsAPIClient.GetMod(query.GameDomain, query.ModId, apiKeyClaim.Value) is not { } modInfo)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Mod not found!"));

            if (validateResponse.UserId != modInfo.User.MemberId)
                return StatusCode((int) HttpStatusCode.Forbidden, new StandardResponse("User does not have access to the mod!"));

            return StatusCode((int) HttpStatusCode.OK, new ModModel(modInfo.Name, modInfo.DomainName, modInfo.ModId));
        }

        [HttpGet("Link")]
        public async Task<ActionResult> Link([FromQuery] LinkModQuery query)
        {
            if (User.IsInRole(ApplicationRoles.Administrator) || User.IsInRole(ApplicationRoles.Moderator))
                return StatusCode((int) HttpStatusCode.OK, new StandardResponse(""));

            if (!HttpContext.User.HasClaim(c => c.Type == "nmapikey") || HttpContext.User.Claims.FirstOrDefault(c => c.Type == "nmapikey") is not { } apiKeyClaim)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Invalid Bearer!"));

            if (await _nexusModsAPIClient.ValidateAPIKey(apiKeyClaim.Value) is not { } validateResponse)
                return StatusCode((int) HttpStatusCode.Unauthorized, new StandardResponse("Invalid NexusMods API Key from Bearer!"));

            if (await _nexusModsAPIClient.GetMod(query.GameDomain, query.ModId, apiKeyClaim.Value) is not { } modInfo)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Mod not found!"));

            if (validateResponse.UserId != modInfo.User.MemberId)
                return StatusCode((int) HttpStatusCode.Forbidden, new StandardResponse("User does not have access to the mod!"));

            if (await _sqlHelperMods.FindAsync(query.GameDomain, query.ModId) is { } mod)
            {
                if (mod.UserIds.Contains(validateResponse.UserId))
                    return StatusCode((int) HttpStatusCode.OK, new StandardResponse("Already linked!"));
            }
            else
            {
                mod = new ModTableEntry
                {
                    Name = modInfo.Name,
                    GameDomain = modInfo.DomainName,
                    ModId = modInfo.ModId,
                    UserIds = new[] { validateResponse.UserId }
                };
            }

            if (await _sqlHelperMods.UpsertAsync(mod with { UserIds = mod.UserIds.Concat(new[] { validateResponse.UserId }).ToArray() }) is not null)
                return StatusCode((int) HttpStatusCode.OK, new StandardResponse("Linked successful!"));

            return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Failed to link!"));
        }

        [HttpGet("Unlink")]
        public async Task<ActionResult> Unlink([FromQuery] LinkModQuery query)
        {
            if (User.IsInRole(ApplicationRoles.Administrator) || User.IsInRole(ApplicationRoles.Moderator))
                return StatusCode((int) HttpStatusCode.OK, new StandardResponse(""));

            if (!HttpContext.User.HasClaim(c => c.Type == "nmapikey") || HttpContext.User.Claims.FirstOrDefault(c => c.Type == "nmapikey") is not { } apiKeyClaim)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Invalid Bearer!"));

            if (await _nexusModsAPIClient.ValidateAPIKey(apiKeyClaim.Value) is not { } validateResponse)
                return StatusCode((int) HttpStatusCode.Unauthorized, new StandardResponse("Invalid NexusMods API Key from Bearer!"));

            if (await _sqlHelperMods.FindAsync(query.GameDomain, query.ModId) is not { } mod)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Mod not found!"));

            if (!mod.UserIds.Contains(validateResponse.UserId))
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Mod is not linked to user!"));

            if (await _sqlHelperMods.UpsertAsync(mod with { UserIds = mod.UserIds.Where(uid => uid != validateResponse.UserId).ToArray() }) is not null)
                return StatusCode((int) HttpStatusCode.OK, new StandardResponse("Unlinked successful!"));

            return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Failed to unlink!"));
        }

        [HttpGet("Refresh")]
        public async Task<ActionResult> Refresh([FromQuery] LinkModQuery query)
        {
            if (User.IsInRole(ApplicationRoles.Administrator) || User.IsInRole(ApplicationRoles.Moderator))
                return StatusCode((int) HttpStatusCode.OK, new StandardResponse(""));

            if (!HttpContext.User.HasClaim(c => c.Type == "nmapikey") || HttpContext.User.Claims.FirstOrDefault(c => c.Type == "nmapikey") is not { } apiKeyClaim)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Invalid Bearer!"));

            if (await _nexusModsAPIClient.ValidateAPIKey(apiKeyClaim.Value) is not { } validateResponse)
                return StatusCode((int) HttpStatusCode.Unauthorized, new StandardResponse("Invalid NexusMods API Key from Bearer!"));

            if (await _nexusModsAPIClient.GetMod(query.GameDomain, query.ModId, apiKeyClaim.Value) is not { } modInfo)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Mod not found!"));

            if (validateResponse.UserId != modInfo.User.MemberId)
                return StatusCode((int) HttpStatusCode.Forbidden, new StandardResponse("User does not have access to the mod!"));

            if (await _sqlHelperMods.FindAsync(query.GameDomain, query.ModId) is { } mod)
            {
                if (await _sqlHelperMods.UpsertAsync(mod with { Name = modInfo.Name }) is not null)
                    return StatusCode((int) HttpStatusCode.OK, new StandardResponse("Updated successful!"));
            }
            return StatusCode((int) HttpStatusCode.NotFound, new StandardResponse("Mod is not linked!"));
        }
    }
}