using BUTR.CrashReportViewer.Shared.Contexts;
using BUTR.CrashReportViewer.Shared.Helpers;
using BUTR.CrashReportViewer.Shared.Models.Contexts;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace BUTR.CrashReportViewer.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ModsController : ControllerBase
    {
        public record LinkModQuery(string GameDomain, int ModId);

        private readonly ILogger<ModsController> _logger;
        private readonly NexusModsAPIClient _nexusModsAPIClient;
        private readonly ModsDbContext _modsDbContext;

        public ModsController(
            ILogger<ModsController> logger,
            NexusModsAPIClient nexusModsAPIClient,
            ModsDbContext modsDbContext)
        {
            _logger = logger ?? throw  new ArgumentNullException(nameof(logger));
            _nexusModsAPIClient = nexusModsAPIClient ?? throw  new ArgumentNullException(nameof(nexusModsAPIClient));
            _modsDbContext = modsDbContext ?? throw  new ArgumentNullException(nameof(modsDbContext));
        }

        [HttpGet("LinkMod")]
        public async Task<ActionResult> LinkMod([FromQuery] LinkModQuery query)
        {
            // We need the NexusMods API Key to confirm we are dealing with a legit User
            // and get his Id which we use to find his mods
            var apiKeyValues = Request.Headers.TryGetValue("apikey", out var val) ? val : StringValues.Empty;
            if (!apiKeyValues.Any())
                return BadRequest("API Key not found!");

            var apiKey = apiKeyValues.First();
            var validateResponse = await _nexusModsAPIClient.ValidateAPIKey(apiKey);
            if (validateResponse == null)
                return Unauthorized("Invalid API Key!");


            var mod = await _modsDbContext.Mods
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.GameDomain == query.GameDomain && m.ModId == query.ModId);

            if (mod == null)
            {
                var modInfo = await _nexusModsAPIClient.GetMod(query.GameDomain, query.ModId, apiKey);
                if (modInfo is null)
                    return NotFound("Mod not found!");

                await _modsDbContext.Mods.AddAsync(new ModTable
                {
                    GameDomain = modInfo.DomainName,
                    ModId = modInfo.ModId,
                    UserIds = new[] { modInfo.User.MemberId }
                });
                await _modsDbContext.SaveChangesAsync();
            }
            else
            {
                if (!mod.UserIds.Contains(validateResponse.UserId))
                    return Forbid("User does not have access to the mod!");

                return Ok("Already linked!");
            }

            return Ok("Linked successful");
        }
    }
}