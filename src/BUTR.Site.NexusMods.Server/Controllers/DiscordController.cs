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
        private readonly DiscordOptions _options;
        private readonly DiscordClient _discordClient;
        private readonly IDiscordStorage _storage;

        public sealed record OauthCallbackQuery(string Code, string State);

        public DiscordController(IOptions<DiscordOptions> options, DiscordClient discordClient, IDiscordStorage storage)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _discordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }
        
        [HttpPost("LinkedRole")]
        //[Produces("application/json")]
        //[ProducesResponseType(typeof(PagingResponse<ExposedModModel>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public ActionResult LinkedRole(CancellationToken ct)
        {
            var (url, state) = _discordClient.GetOAuthUrl();
            HttpContext.Response.Cookies.Append("clientState", state.ToString());
            return Redirect(url);
        }
        
        [HttpPost("OAuthCallback")]
        //[Produces("application/json")]
        //[ProducesResponseType(typeof(PagingResponse<ExposedModModel>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> OAuthCallback([FromBody] OauthCallbackQuery query, CancellationToken ct)
        {
            if (!HttpContext.Request.Cookies.TryGetValue("clientState", out var state))
                return StatusCode(StatusCodes.Status403Forbidden, "State verification failed.");

            var tokens = await _discordClient.GetOAuthTokens(query.Code);

            var userInfo = await _discordClient.GetUserInfo(tokens.AccessToken);
            _storage.Upsert(userInfo.User.Id, new DiscordOAuthTokens(tokens.AccessToken, tokens.RefreshToken, DateTimeOffset.Now + TimeSpan.FromSeconds(tokens.ExpiresIn)));

            await UpdateMetadata(userInfo.User.Id);
            
            return Ok();
        }


        private async Task UpdateMetadata(int userId)
        {
            var role = HttpContext.GetRole();
            
            var tokens = _storage.Get(userId);
            var metadataJson = $@"{{
  ""butrmodauthor"": 1,
  ""butrmoderator"": {(role == ApplicationRoles.Moderator ? 1 : 0)},
  ""butradministrator"": {(role == ApplicationRoles.Administrator ? 1 : 0)},
}}";
            await _discordClient.PushMetadata(userId, tokens, metadataJson);
        }
    }
}