using BUTR.CrashReportViewer.Shared.Contexts;
using BUTR.CrashReportViewer.Shared.Helpers;
using BUTR.CrashReportViewer.Shared.Models;
using BUTR.CrashReportViewer.Shared.Models.Contexts;

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
    public class ModsController : ControllerBase
    {
        public record LinkModQuery(string GameDomain, int ModId);

        private readonly ILogger _logger;
        private readonly NexusModsAPIClient _nexusModsAPIClient;
        private readonly MainDbContext _mainDbContext;

        public ModsController(ILogger<ModsController> logger, NexusModsAPIClient nexusModsAPIClient, MainDbContext mainDbContext)
        {
            _logger = logger ?? throw  new ArgumentNullException(nameof(logger));
            _nexusModsAPIClient = nexusModsAPIClient ?? throw  new ArgumentNullException(nameof(nexusModsAPIClient));
            _mainDbContext = mainDbContext ?? throw  new ArgumentNullException(nameof(mainDbContext));
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromHeader] string? apiKey)
        {
            if (apiKey is null)
                return StatusCode((int) HttpStatusCode.BadRequest, "API Key not found!");

            var validateResponse = await _nexusModsAPIClient.ValidateAPIKey(apiKey);
            if (validateResponse == null)
                return StatusCode((int) HttpStatusCode.Unauthorized, "Invalid API Key!");

            var userMods = _mainDbContext.Mods
                .AsNoTracking()
                .AsEnumerable()
                .Where(m => m.UserIds.Contains(validateResponse.UserId));

            var modModels = userMods
                .Select(m => new ModModel
                {
                    Name = m.Name,
                    GameDomain = m.GameDomain,
                    ModId = m.ModId,
                })
                .OrderBy(m => m.ModId);
            return StatusCode((int) HttpStatusCode.OK, modModels);
        }

        [HttpGet("LinkMod")]
        public async Task<ActionResult> LinkMod([FromHeader] string? apiKey, [FromQuery] LinkModQuery query)
        {
            if (apiKey is null)
                return StatusCode((int) HttpStatusCode.BadRequest, "API Key not found!");

            var validateResponse = await _nexusModsAPIClient.ValidateAPIKey(apiKey);
            if (validateResponse == null)
                return StatusCode((int) HttpStatusCode.Unauthorized, "Invalid API Key!");

            var modInfoResponse = await _nexusModsAPIClient.GetMod(query.GameDomain, query.ModId, apiKey);
            if (modInfoResponse == null)
                return StatusCode((int) HttpStatusCode.BadRequest, "Mod not found!");


            var mod = await _mainDbContext.Mods
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.GameDomain == query.GameDomain && m.ModId == query.ModId);

            if (mod == null)
            {
                var modInfo = await _nexusModsAPIClient.GetMod(query.GameDomain, query.ModId, apiKey);
                if (modInfo is null)
                    return StatusCode((int) HttpStatusCode.NotFound, "Mod not found!");

                if (validateResponse.UserId != modInfoResponse.User.MemberId)
                    return StatusCode((int) HttpStatusCode.Forbidden, "User does not have access to the mod!");

                await _mainDbContext.Mods.AddAsync(new ModTable
                {
                    Name = modInfo.Name,
                    GameDomain = modInfo.DomainName,
                    ModId = modInfo.ModId,
                    UserIds = new[] { modInfo.User.MemberId }
                });
                await _mainDbContext.SaveChangesAsync();
            }
            else
            {
                if (!mod.UserIds.Contains(validateResponse.UserId))
                    return StatusCode((int) HttpStatusCode.Forbidden, "User does not have access to the mod!");

                return StatusCode((int) HttpStatusCode.OK, "Already linked!");
            }

            return StatusCode((int) HttpStatusCode.OK, "Linked successful!");
        }
    }
}