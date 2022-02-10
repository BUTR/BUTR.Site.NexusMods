using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Authentication.NexusMods.Services;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Services.Database;
using BUTR.Site.NexusMods.Shared.Helpers;
using BUTR.Site.NexusMods.Shared.Models;
using BUTR.Site.NexusMods.Shared.Models.API;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers
{
    [ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly NexusModsAPIClient _nexusModsAPIClient;
        private readonly RoleProvider _roleProvider;
        private readonly ITokenGenerator _tokenGenerator;

        public AuthenticationController(
            ILogger<AuthenticationController> logger,
            NexusModsAPIClient nexusModsAPIClient,
            RoleProvider roleProvider,
            ITokenGenerator tokenGenerator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _nexusModsAPIClient = nexusModsAPIClient ?? throw new ArgumentNullException(nameof(nexusModsAPIClient));
            _roleProvider = roleProvider ?? throw new ArgumentNullException(nameof(roleProvider));
            _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
        }

        [HttpGet("Authenticate"), AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(typeof(JwtTokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Authenticate([FromHeader] string? apiKey)
        {
            if (apiKey is null)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("API Key not found!"));

            if (await _nexusModsAPIClient.ValidateAPIKey(apiKey) is not { } validateResponse)
                return StatusCode(StatusCodes.Status401Unauthorized, new StandardResponse("Invalid NexusMods API Key!"));

            var role = (await _roleProvider.FindAsync(validateResponse.UserId))?.Role ?? ApplicationRoles.User;
            return StatusCode(StatusCodes.Status200OK, new JwtTokenResponse(await _tokenGenerator.GenerateTokenAsync(new ButrNexusModsUserInfo
            {
                UserId = validateResponse.UserId,
                Name = validateResponse.Name,
                EMail = validateResponse.Email,
                ProfileUrl = validateResponse.ProfileUrl,
                IsSupporter = validateResponse.IsSupporter,
                IsPremium = validateResponse.IsPremium,
                APIKey = validateResponse.Key,
                Role = role
            }), GetProfile(HttpContext)));
        }

        [HttpGet("Profile")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ProfileModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public ActionResult Profile() => StatusCode(StatusCodes.Status200OK, GetProfile(HttpContext));

        private static ProfileModel GetProfile(HttpContext context) => new(
            context.GetUserId(),
            context.GetName(),
            context.GetEMail(),
            context.GetProfileUrl(),
            context.GetIsPremium(),
            context.GetIsSupporter(),
            context.GetRole());
    }
}