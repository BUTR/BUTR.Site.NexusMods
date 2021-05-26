using BUTR.CrashReportViewer.Server.Helpers;
using BUTR.CrashReportViewer.Server.Models.NexusModsAPI;
using BUTR.CrashReportViewer.Server.Options;
using BUTR.CrashReportViewer.Shared.Helpers;
using BUTR.CrashReportViewer.Shared.Models;
using BUTR.CrashReportViewer.Shared.Models.API;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BUTR.CrashReportViewer.Server.Controllers
{
    [ApiController, Route("[controller]"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AuthenticationController : ControllerBase
    {
        private static readonly ProfileModel Administrator = new(-1, "Administrator", "admin@butr.dev", "{BASE}images/default_profile.webp", false, false);

        private readonly ILogger _logger;
        private readonly NexusModsAPIClient _nexusModsAPIClient;
        private readonly JwtOptions _jwtOptions;
        private readonly AuthenticationOptions _authenticationOptions;

        public AuthenticationController(
            ILogger<AuthenticationController> logger,
            NexusModsAPIClient nexusModsAPIClient,
            IOptions<JwtOptions> jwtOptions,
            IOptions<AuthenticationOptions> authenticationOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _nexusModsAPIClient = nexusModsAPIClient ?? throw new ArgumentNullException(nameof(nexusModsAPIClient));
            _jwtOptions = jwtOptions.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
            _authenticationOptions = authenticationOptions.Value ?? throw new ArgumentNullException(nameof(authenticationOptions));
        }

        [HttpGet("Authenticate"), AllowAnonymous]
        public async Task<ActionResult> Authenticate([FromHeader] string? apiKey, [FromQuery] string? type)
        {
            if (apiKey is null)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("API Key not found!"));

            if (type?.Equals("admin", StringComparison.OrdinalIgnoreCase) == true)
            {
                if (apiKey.Equals(_authenticationOptions.AdminToken, StringComparison.Ordinal))
                {
                    return Ok(new JwtTokenResponse(GenerateJsonWebToken(new NexusModsValidateResponse
                    {
                        UserId = Administrator.UserId,
                        Name = Administrator.Name,
                        Email = Administrator.Email
                    })));
                }

                return StatusCode((int) HttpStatusCode.Unauthorized, new StandardResponse("Invalid Administrator Token!"));
            }

            if (await _nexusModsAPIClient.ValidateAPIKey(apiKey) is not { } validateResponse)
                return StatusCode((int) HttpStatusCode.Unauthorized, new StandardResponse("Invalid NexusMods API Key!"));

            return Ok(new JwtTokenResponse(GenerateJsonWebToken(validateResponse)));
        }

        [HttpGet("Validate")]
        public async Task<ActionResult> Validate()
        {
            if (User.IsInRole(ApplicationRoles.Administrator) || User.IsInRole(ApplicationRoles.Moderator))
                return Ok();

            if (!HttpContext.User.HasClaim(c => c.Type == "nmapikey") || HttpContext.User.Claims.FirstOrDefault(c => c.Type == "nmapikey") is not { } apiKeyClaim)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Invalid Bearer!"));

            if (await _nexusModsAPIClient.ValidateAPIKey(apiKeyClaim.Value) is null)
                return StatusCode((int) HttpStatusCode.Unauthorized, new StandardResponse("Invalid NexusMods API Key from Bearer!"));

            return Ok();
        }

        [HttpGet("Profile")]
        public async Task<ActionResult> Profile()
        {
            if (User.IsInRole(ApplicationRoles.Administrator) || User.IsInRole(ApplicationRoles.Moderator))
                return Ok(Administrator);

            if (!HttpContext.User.HasClaim(c => c.Type == "nmapikey") || HttpContext.User.Claims.FirstOrDefault(c => c.Type == "nmapikey") is not { } apiKeyClaim)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Invalid Bearer!"));

            if (await _nexusModsAPIClient.ValidateAPIKey(apiKeyClaim.Value) is not { } vr)
                return StatusCode((int) HttpStatusCode.Unauthorized, new StandardResponse("Invalid NexusMods API Key from Bearer!"));

            return Ok(new ProfileModel(vr.UserId, vr.Name, vr.Email, vr.ProfileUrl, vr.IsPremium, vr.IsSupporter));
        }


        private string GenerateJsonWebToken(NexusModsValidateResponse userInfo)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = GetTokenDescriptor(userInfo);
            return tokenHandler.CreateEncodedJwt(tokenDescriptor);
        }
        private SecurityTokenDescriptor GetTokenDescriptor(NexusModsValidateResponse userInfo)
        {
            const int expiringDays = 7;
            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SignKey)), SecurityAlgorithms.HmacSha256Signature);
            var encryptingCredentials = new EncryptingCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.EncryptionKey)), SecurityAlgorithms.Aes128KW, SecurityAlgorithms.Aes128CbcHmacSha256);

            return new SecurityTokenDescriptor
            {
                Claims = new Dictionary<string, object>
                {
                    { ClaimTypes.NameIdentifier, userInfo.UserId.ToString() },
                    { JwtRegisteredClaimNames.Sub, userInfo.Name },
                    { JwtRegisteredClaimNames.Email, userInfo.Email },
                    { "nmapikey", userInfo.Key },
                    { ClaimTypes.Role, new [] { ApplicationRoles.User, userInfo.UserId == -1 ? ApplicationRoles.Administrator : string.Empty } },
                },
                SigningCredentials = signingCredentials,
                EncryptingCredentials = encryptingCredentials,
                Expires = DateTime.UtcNow.AddDays(expiringDays),
            };
        }
    }
}