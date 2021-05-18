using BUTR.CrashReportViewer.Shared.Helpers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
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

        public NexusModsAPIProxyController(ILogger<NexusModsAPIProxyController> logger, NexusModsAPIClient nexusModsAPIClient)
        {
            _logger = logger ?? throw  new ArgumentNullException(nameof(logger));
            _nexusModsAPIClient = nexusModsAPIClient ?? throw  new ArgumentNullException(nameof(nexusModsAPIClient));
        }

        [HttpGet("v1/users/validate.json")]
        public async Task<ActionResult> Validate([FromHeader] string? apiKey)
        {
            if (apiKey is null)
                return StatusCode((int) HttpStatusCode.BadRequest, "API Key not found!");

            var validateResponse = await _nexusModsAPIClient.ValidateAPIKey(apiKey);
            if (validateResponse == null)
                return StatusCode((int) HttpStatusCode.Unauthorized, "Invalid API Key!");

            return Ok(validateResponse);
        }

        [HttpGet("v1/games/{gameDomainName}/mods/{modId}.json")]
        public async Task<ActionResult> ModInfo([FromHeader] string? apiKey, string gameDomainName, int modId)
        {
            if (apiKey is null)
                return StatusCode((int) HttpStatusCode.BadRequest, "API Key not found!");

            var modInfoResponse = await _nexusModsAPIClient.GetMod(gameDomainName, modId, apiKey);
            if (modInfoResponse == null)
                return StatusCode((int) HttpStatusCode.BadRequest, "Invalid API Key or Mod not found!");

            return Ok(modInfoResponse);
        }
    }
}