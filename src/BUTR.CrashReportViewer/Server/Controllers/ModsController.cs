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

        private readonly ILogger _logger;
        private readonly NexusModsAPIClient _nexusModsAPIClient;
        private readonly MainDbContext _mainDbContext;

        public ModsController(ILogger<ModsController> logger, NexusModsAPIClient nexusModsAPIClient, MainDbContext mainDbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _nexusModsAPIClient = nexusModsAPIClient ?? throw new ArgumentNullException(nameof(nexusModsAPIClient));
            _mainDbContext = mainDbContext ?? throw new ArgumentNullException(nameof(mainDbContext));
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            if (!HttpContext.User.HasClaim(c => c.Type == "nmapikey") || HttpContext.User.Claims.FirstOrDefault(c => c.Type == "nmapikey") is not { } apiKeyClaim)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Invalid Bearer!"));

            if (await _nexusModsAPIClient.ValidateAPIKey(apiKeyClaim.Value) is not { } validateResponse)
                return StatusCode((int) HttpStatusCode.Unauthorized, new StandardResponse("Invalid NexusMods API Key from Bearer!"));

            var userMods = _mainDbContext.Mods
                .AsNoTracking()
                .AsEnumerable()
                .Where(m => m.UserIds.Contains(validateResponse.UserId));

            var modModels = userMods
                .Select(m => new ModModel(m.Name, m.GameDomain, m.ModId))
                .OrderBy(m => m.ModId);
            return StatusCode((int) HttpStatusCode.OK, modModels);
        }

        [HttpGet("GetMod")]
        public async Task<ActionResult> GetMod([FromQuery] LinkModQuery query)
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

        [HttpGet("LinkMod")]
        public async Task<ActionResult> LinkMod([FromQuery] LinkModQuery query)
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

        [HttpGet("UnlinkMod")]
        public async Task<ActionResult> UnlinkMod([FromQuery] LinkModQuery query)
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

        [HttpGet("RefreshMod")]
        public async Task<ActionResult> RefreshMod([FromQuery] LinkModQuery query)
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