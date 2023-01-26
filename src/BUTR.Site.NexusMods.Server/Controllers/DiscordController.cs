using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers
{
    [ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
    public sealed class DiscordController : ControllerBase
    {
        private sealed record Metadata(
            [property: JsonPropertyName(DiscordConstants.BUTRModAuthor)] int IsModAuthor,
            [property: JsonPropertyName(DiscordConstants.BUTRModerator)] int IsModerator,
            [property: JsonPropertyName(DiscordConstants.BUTRAdministrator)] int IsAdministrator);
        
        private readonly DiscordClient _discordClient;

        public DiscordController(DiscordClient discordClient)
        {
            _discordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
        }
        
        [HttpGet("GetOAuthUrl")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(DiscordOAuthUrlModel), StatusCodes.Status200OK)]
        public ActionResult GetOAuthUrl()
        {
            var (url, state) = _discordClient.GetOAuthUrl();
            return StatusCode(StatusCodes.Status200OK, new DiscordOAuthUrlModel(url, state));
        }
        
        [HttpGet("GetOAuthTokens")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(DiscordOAuthTokens), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetOAuthTokens([FromQuery] string code, CancellationToken ct)
        {
            var tokens = await _discordClient.GetOAuthTokens(code);
            return StatusCode(tokens is not null ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest, tokens);

        }

        [HttpPost("UpdateMetadata")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateMetadata([FromBody] DiscordAccessTokenModel body)
        {
            var role = HttpContext.GetRole();

            var result = await _discordClient.PushMetadata(body.AccessToken, new Metadata(1, role == ApplicationRoles.Moderator ? 1 : 0, role == ApplicationRoles.Administrator ? 1 : 0));
            return StatusCode(result ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest);
        }
        
        
        [HttpPost("GetUserInfo")]
        [ProducesResponseType(typeof(DiscordUserInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetUserInfo([FromBody] DiscordAccessTokenModel body)
        {
            var result = await _discordClient.GetUserInfo(body.AccessToken);
            return StatusCode(result is null ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest, result);
        }
    }
}