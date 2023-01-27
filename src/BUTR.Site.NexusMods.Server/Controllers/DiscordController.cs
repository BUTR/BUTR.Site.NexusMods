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
using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
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
            [property: JsonPropertyName(DiscordConstants.BUTRAdministrator)] int IsAdministrator,
            [property: JsonPropertyName(DiscordConstants.BUTRLinkedMods)] int LinkedMods);

        private readonly DiscordClient _discordClient;
        private readonly AppDbContext _dbContext;

        public DiscordController(DiscordClient discordClient, AppDbContext dbContext)
        {
            _discordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
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
            var userId = HttpContext.GetUserId();

            var linkedModsCount = await _dbContext
                .Set<NexusModsModEntity>()
                .CountAsync(y => y.UserIds.Contains(userId));
            var manuallyLinkedModsCount = await _dbContext
                .Set<UserAllowedModsEntity>()
                .CountAsync(y => y.UserId == userId);

            var result = await _discordClient.PushMetadata(body.AccessToken, new Metadata(
                1,
                role == ApplicationRoles.Moderator ? 1 : 0,
                role == ApplicationRoles.Administrator ? 1 : 0,
                linkedModsCount + manuallyLinkedModsCount));
            return StatusCode(result ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest);
        }


        [HttpPost("GetUserInfo")]
        [ProducesResponseType(typeof(DiscordUserInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetUserInfo([FromBody] DiscordAccessTokenModel body)
        {
            var result = await _discordClient.GetUserInfo(body.AccessToken);
            return StatusCode(result is not null ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest, result);
        }
    }
}