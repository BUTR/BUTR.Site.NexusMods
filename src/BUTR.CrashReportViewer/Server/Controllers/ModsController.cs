using BUTR.CrashReportViewer.Server.Contexts;
using BUTR.CrashReportViewer.Server.Helpers;
using BUTR.CrashReportViewer.Server.Models.Contexts;
using BUTR.CrashReportViewer.Shared.Models;
using BUTR.CrashReportViewer.Shared.Models.API;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BUTR.CrashReportViewer.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ModsController : ControllerBase
    {
        public record LinkModQuery(string GameDomain, int ModId);
        public record ModsQuery(int Page, int PageSize);

        private readonly ILogger _logger;
        private readonly NexusModsAPIClient _nexusModsAPIClient;
        private readonly MainDbContext _mainDbContext;

        public ModsController(ILogger<ModsController> logger, NexusModsAPIClient nexusModsAPIClient, MainDbContext mainDbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _nexusModsAPIClient = nexusModsAPIClient ?? throw new ArgumentNullException(nameof(nexusModsAPIClient));
            _mainDbContext = mainDbContext ?? throw new ArgumentNullException(nameof(mainDbContext));
        }

        [HttpGet("")]
        public async Task<ActionResult> GetAll([FromQuery] ModsQuery query)
        {
            var page = query.Page;
            var pageSize = Math.Max(Math.Min(query.PageSize, 20), 5);

            if (!HttpContext.User.HasClaim(c => c.Type == "nmapikey") || HttpContext.User.Claims.FirstOrDefault(c => c.Type == "nmapikey") is not { } apiKeyClaim)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Invalid Bearer!"));

            if (await _nexusModsAPIClient.ValidateAPIKey(apiKeyClaim.Value) is not { } validateResponse)
                return StatusCode((int) HttpStatusCode.Unauthorized, new StandardResponse("Invalid NexusMods API Key from Bearer!"));

            var userModCount = _mainDbContext.Mods
                .AsNoTracking()
                .Count(m => m.UserIds.Contains(validateResponse.UserId));

            var userMods = _mainDbContext.Mods
                .AsNoTracking()
                .OrderBy(m => m.ModId)
                .Where(m => m.UserIds.Contains(validateResponse.UserId))
                .Select(m => new ModModel(m.Name, m.GameDomain, m.ModId))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var metadata = new PagingMetadata
            {
                PageSize = pageSize,
                CurrentPage = page,
                TotalCount = userModCount,
                TotalPages = (int) Math.Ceiling((double) userModCount / (double) pageSize),
            };

            return StatusCode((int) HttpStatusCode.OK, new PagingResponse<ModModel>
            {
                Items = userMods,
                Metadata = metadata
            });
        }

        [HttpGet("Get")]
        public async Task<ActionResult> Get([FromQuery] LinkModQuery query)
        {
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
            if (!HttpContext.User.HasClaim(c => c.Type == "nmapikey") || HttpContext.User.Claims.FirstOrDefault(c => c.Type == "nmapikey") is not { } apiKeyClaim)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Invalid Bearer!"));

            if (await _nexusModsAPIClient.ValidateAPIKey(apiKeyClaim.Value) is not { } validateResponse)
                return StatusCode((int) HttpStatusCode.Unauthorized, new StandardResponse("Invalid NexusMods API Key from Bearer!"));

            if (await _nexusModsAPIClient.GetMod(query.GameDomain, query.ModId, apiKeyClaim.Value) is not { } modInfo)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Mod not found!"));

            if (validateResponse.UserId != modInfo.User.MemberId)
                return StatusCode((int) HttpStatusCode.Forbidden, new StandardResponse("User does not have access to the mod!"));

            if (await _mainDbContext.Mods.FindAsync(query.GameDomain, query.ModId) is not { } mod)
            {
                await _mainDbContext.Mods.AddAsync(new ModTable
                {
                    Name = modInfo.Name,
                    GameDomain = modInfo.DomainName,
                    ModId = modInfo.ModId,
                    UserIds = new[] { validateResponse.UserId }
                });
                await _mainDbContext.SaveChangesAsync();
            }
            else
            {
                if (mod.UserIds.Contains(validateResponse.UserId))
                    return StatusCode((int) HttpStatusCode.OK, new StandardResponse("Already linked!"));

                mod.UserIds = mod.UserIds.Concat(new[] { validateResponse.UserId }).ToArray();
                await _mainDbContext.SaveChangesAsync();
            }

            return StatusCode((int) HttpStatusCode.OK, new StandardResponse("Linked successful!"));
        }

        [HttpGet("Unlink")]
        public async Task<ActionResult> Unlink([FromQuery] LinkModQuery query)
        {
            if (!HttpContext.User.HasClaim(c => c.Type == "nmapikey") || HttpContext.User.Claims.FirstOrDefault(c => c.Type == "nmapikey") is not { } apiKeyClaim)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Invalid Bearer!"));

            if (await _nexusModsAPIClient.ValidateAPIKey(apiKeyClaim.Value) is not { } validateResponse)
                return StatusCode((int) HttpStatusCode.Unauthorized, new StandardResponse("Invalid NexusMods API Key from Bearer!"));

            if (await _mainDbContext.Mods.FindAsync(query.GameDomain, query.ModId) is not { } mod)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Mod not found!"));

            if (!mod.UserIds.Contains(validateResponse.UserId))
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Mod is not linked to user!"));

            mod.UserIds = mod.UserIds.Where(uid => uid != validateResponse.UserId).ToArray();
            _mainDbContext.Mods.Update(mod);
            await _mainDbContext.SaveChangesAsync();

            return StatusCode((int) HttpStatusCode.OK, new StandardResponse("Unlinked successful!"));
        }

        [HttpGet("Refresh")]
        public async Task<ActionResult> Refresh([FromQuery] LinkModQuery query)
        {
            if (!HttpContext.User.HasClaim(c => c.Type == "nmapikey") || HttpContext.User.Claims.FirstOrDefault(c => c.Type == "nmapikey") is not { } apiKeyClaim)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Invalid Bearer!"));

            if (await _nexusModsAPIClient.ValidateAPIKey(apiKeyClaim.Value) is not { } validateResponse)
                return StatusCode((int) HttpStatusCode.Unauthorized, new StandardResponse("Invalid NexusMods API Key from Bearer!"));

            if (await _nexusModsAPIClient.GetMod(query.GameDomain, query.ModId, apiKeyClaim.Value) is not { } modInfo)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Mod not found!"));

            if (validateResponse.UserId != modInfo.User.MemberId)
                return StatusCode((int) HttpStatusCode.Forbidden, new StandardResponse("User does not have access to the mod!"));

            if (await _mainDbContext.Mods.FindAsync(query.GameDomain, query.ModId) is { } mod)
            {
                mod.Name = modInfo.Name;
                await _mainDbContext.SaveChangesAsync();
                return StatusCode((int) HttpStatusCode.OK, new StandardResponse("Updated successful!"));
            }
            return StatusCode((int) HttpStatusCode.NotFound, new StandardResponse("Mod is not linked!"));
        }
    }
}