using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Options;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers
{
    [ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
    public sealed class DiscordController : ControllerBase
    {
        private readonly DiscordClient _discordClient;

        public sealed record UpdateMetadataRequest(string AccessToken);
        public sealed record DiscordOAuthUrlResponse(string Url, Guid State);

        public DiscordController(DiscordClient discordClient)
        {
            _discordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
        }
        
        [HttpGet("GetOAuthUrl")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(DiscordOAuthUrlResponse), StatusCodes.Status200OK)]
        public ActionResult GetOAuthUrl()
        {
            var (url, state) = _discordClient.GetOAuthUrl();
            return StatusCode(StatusCodes.Status200OK, new DiscordOAuthUrlResponse(url, state));
        }
        
        [HttpGet("GetOAuthTokens")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(DiscordOAuthTokensResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetOAuthTokens([FromQuery] string code, CancellationToken ct)
        {
            var tokens = await _discordClient.GetOAuthTokens(code);
            return StatusCode(StatusCodes.Status200OK, tokens);

        }

        [HttpPost("UpdateMetadata")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> UpdateMetadata([FromBody] UpdateMetadataRequest body)
        {
            var role = HttpContext.GetRole();
            
            var metadataJson = $@"{{
  ""butrmodauthor"": 1,
  ""butrmoderator"": {(role == ApplicationRoles.Moderator ? 1 : 0)},
  ""butradministrator"": {(role == ApplicationRoles.Administrator ? 1 : 0)},
}}";
            await _discordClient.PushMetadata(body.AccessToken, metadataJson);
            return StatusCode(StatusCodes.Status200OK);
        }
    }
}