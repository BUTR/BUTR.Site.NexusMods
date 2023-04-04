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
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers
{
    [ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
    public sealed class DiscordController : ControllerExtended
    {
        private sealed record Metadata(
            [property: JsonPropertyName(DiscordConstants.BUTRModAuthor)] int IsModAuthor,
            [property: JsonPropertyName(DiscordConstants.BUTRModerator)] int IsModerator,
            [property: JsonPropertyName(DiscordConstants.BUTRAdministrator)] int IsAdministrator,
            [property: JsonPropertyName(DiscordConstants.BUTRLinkedMods)] int LinkedMods);

        private readonly DiscordClient _discordClient;
        private readonly IDiscordStorage _discordStorage;
        private readonly AppDbContext _dbContext;

        public DiscordController(DiscordClient discordClient, IDiscordStorage discordStorage, AppDbContext dbContext)
        {
            _discordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
            _discordStorage = discordStorage ?? throw new ArgumentNullException(nameof(discordStorage));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        [HttpGet("GetOAuthUrl")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(APIResponse<DiscordOAuthUrlModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public ActionResult<APIResponse<DiscordOAuthUrlModel?>> GetOAuthUrl()
        {
            var (url, state) = _discordClient.GetOAuthUrl();
            return Result(APIResponse.From(new DiscordOAuthUrlModel(url, state)));
        }

        [HttpGet("Link")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse<string?>>> Link([FromQuery] string code)
        {
            var tokens = await _discordClient.GetOAuthTokens(code);
            if (tokens is null)
                return Result(APIResponse.Error<string>("Failed to link!"));

            var userId = HttpContext.GetUserId();
            var userInfo = await _discordClient.GetUserInfo(tokens.AccessToken);

            if (userInfo is null || !_discordStorage.Upsert(userId, userInfo.User.Id, tokens))
                return Result(APIResponse.Error<string>("Failed to link!"));

            await UpdateMetadataInternal();

            return Result(APIResponse.From("Linked successful!"));
        }

        [HttpPost("Unlink")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse<string?>>> Unlink()
        {
            var userId = HttpContext.GetUserId();
            var tokens = HttpContext.GetDiscordTokens();

            if (tokens is null)
                return Result(APIResponse.Error<string>("Unlinked successful!"));

            if (!await _discordClient.PushMetadata(userId, tokens.UserId, new DiscordOAuthTokens(tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresAt), new Metadata(0, 0, 0, 0)))
                return Result(APIResponse.Error<string>("Failed to unlink!"));

            if (!_discordStorage.Remove(userId, tokens.UserId))
                return Result(APIResponse.From("Failed to unlink!"));

            return Result(APIResponse.From("Unlinked successful!"));

        }

        [HttpPost("UpdateMetadata")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse<string?>>> UpdateMetadata()
        {
            if (await UpdateMetadataInternal() is not { } result)
                return Result(APIResponse.Error<string>("Failed to update"));
            return Result(result ? APIResponse.From("") : APIResponse.Error<string>("Failed to update"));
        }


        [HttpPost("GetUserInfo")]
        [ProducesResponseType(typeof(APIResponse<DiscordUserInfo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse<DiscordUserInfo?>>> GetUserInfoByAccessToken()
        {
            var userId = HttpContext.GetUserId();
            var tokens = HttpContext.GetDiscordTokens();

            if (tokens is null)
                return Result(APIResponse.Error<DiscordUserInfo>("Failed to get the token!"));

            var result = await _discordClient.GetUserInfo(userId, tokens.UserId, new DiscordOAuthTokens(tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresAt));
            return Result(APIResponse.From(result));
        }

        private async Task<bool?> UpdateMetadataInternal()
        {
            var role = HttpContext.GetRole();
            var userId = HttpContext.GetUserId();
            var tokens = HttpContext.GetDiscordTokens();

            if (tokens is null)
                return null;

            var linkedModsCount = await _dbContext
                .Set<NexusModsModEntity>()
                .CountAsync(y => y.UserIds.Contains(userId));
            var manuallyLinkedModsCount = await _dbContext
                .Set<UserAllowedModsEntity>()
                .CountAsync(y => y.UserId == userId);

            return await _discordClient.PushMetadata(userId, tokens.UserId, new DiscordOAuthTokens(tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresAt), new Metadata(
                1,
                role == ApplicationRoles.Moderator ? 1 : 0,
                role == ApplicationRoles.Administrator ? 1 : 0,
                linkedModsCount + manuallyLinkedModsCount));
        }
    }
}