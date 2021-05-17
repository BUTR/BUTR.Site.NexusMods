using BUTR.CrashReportViewer.Shared.Helpers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BUTR.CrashReportViewer.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NexusModsAPIProxyController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly NexusModsAPIClient _nexusModsAPIClient;

        public NexusModsAPIProxyController(
            ILogger<NexusModsAPIProxyController> logger,
            NexusModsAPIClient nexusModsAPIClient)
        {
            _logger = logger ?? throw  new ArgumentNullException(nameof(logger));
            _nexusModsAPIClient = nexusModsAPIClient ?? throw  new ArgumentNullException(nameof(nexusModsAPIClient));
        }

        [HttpGet("v1/users/validate.json")]
        public async Task<ActionResult> Validate()
        {
            // We need the NexusMods API Key to confirm we are dealing with a legit User
            // and get his Id which we use to find his mods
            var apiKeyValues = Request.Headers.TryGetValue("apikey", out var val) ? val : StringValues.Empty;
            if (!apiKeyValues.Any())
                return StatusCode((int) HttpStatusCode.BadRequest, "API Key not found!");

            var apiKey = apiKeyValues.First();
            var validateResponse = await _nexusModsAPIClient.ValidateAPIKey(apiKey);
            if (validateResponse == null)
                return StatusCode((int) HttpStatusCode.Unauthorized, "Invalid API Key!");

            return Ok(validateResponse);
        }

        [HttpGet("v1/games/{game_domain_name}/mods/{id}.json")]
        public async Task<ActionResult> ModInfo(string game_domain_name, int id)
        {
            // We need the NexusMods API Key to confirm we are dealing with a legit User
            // and get his Id which we use to find his mods
            var apiKeyValues = Request.Headers.TryGetValue("apikey", out var val) ? val : StringValues.Empty;
            if (!apiKeyValues.Any())
                return StatusCode((int) HttpStatusCode.BadRequest, "API Key not found!");

            var apiKey = apiKeyValues.First();
            var modInfoResponse = await _nexusModsAPIClient.GetMod(game_domain_name, id, apiKey);
            if (modInfoResponse == null)
                return StatusCode((int) HttpStatusCode.BadRequest, "Invalid API Key or Mod not found!");

            return Ok(modInfoResponse);
        }
    }
}