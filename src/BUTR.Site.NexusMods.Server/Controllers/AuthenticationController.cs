using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Authentication.NexusMods.Services;
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
using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
public sealed class AuthenticationController : ControllerExtended
{
    private readonly ILogger _logger;
    private readonly NexusModsAPIClient _nexusModsAPIClient;
    private readonly AppDbContext _dbContext;
    private readonly ITokenGenerator _tokenGenerator;

    public AuthenticationController(
        ILogger<AuthenticationController> logger,
        NexusModsAPIClient nexusModsAPIClient,
        AppDbContext dbContext,
        ITokenGenerator tokenGenerator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _nexusModsAPIClient = nexusModsAPIClient ?? throw new ArgumentNullException(nameof(nexusModsAPIClient));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
    }

    [HttpGet("Authenticate"), AllowAnonymous]
    [Produces("application/json")]
    [ProducesResponseType(typeof(APIResponse<JwtTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<APIResponse<JwtTokenResponse?>>> AuthenticateAsync([FromHeader] string? apiKey, CancellationToken ct)
    {
        if (apiKey is null)
            return StatusCode(StatusCodes.Status401Unauthorized);

        if (await _nexusModsAPIClient.ValidateAPIKeyAsync(apiKey, ct) is not { } validateResponse)
            return StatusCode(StatusCodes.Status401Unauthorized);

        var roleEntity = await _dbContext.Set<NexusModsUserRoleEntity>().AsNoTracking().Prepare().FirstOrDefaultAsync(x => x.NexusModsUserId == validateResponse.UserId, ct);
        var role = roleEntity?.Role ?? ApplicationRoles.User;
        var metadataEntity = await _dbContext.Set<NexusModsUserMetadataEntity>().AsNoTracking().Prepare().FirstOrDefaultAsync(x => x.NexusModsUserId == validateResponse.UserId, ct);
        var metadata = metadataEntity?.Metadata ?? new();

        var generatedToken = await _tokenGenerator.GenerateTokenAsync(new ButrNexusModsUserInfo
        {
            UserId = validateResponse.UserId,
            Name = validateResponse.Name,
            EMail = validateResponse.Email,
            ProfileUrl = validateResponse.ProfileUrl,
            IsSupporter = validateResponse.IsSupporter,
            IsPremium = validateResponse.IsPremium,
            APIKey = validateResponse.Key,
            Role = role,
            Metadata = metadata
        });
        return APIResponse(new JwtTokenResponse(generatedToken.Token, HttpContext.GetProfile(role)));
    }

    [HttpGet("Validate"), AllowAnonymous]
    [Produces("application/json")]
    [ProducesResponseType(typeof(APIResponse<JwtTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<APIResponse<JwtTokenResponse?>>> ValidateAsync(CancellationToken ct)
    {
        if (HttpContext.GetAPIKey() is not { } apikey || string.IsNullOrEmpty(apikey) || await _nexusModsAPIClient.ValidateAPIKeyAsync(apikey, ct) is not { } validateResponse)
            return StatusCode(StatusCodes.Status401Unauthorized);

        var roleEntity = await _dbContext.Set<NexusModsUserRoleEntity>().AsNoTracking().Prepare().FirstOrDefaultAsync(x => x.NexusModsUserId == validateResponse.UserId, ct);
        var role = roleEntity?.Role ?? ApplicationRoles.User;

        var metadataEntity = await _dbContext.Set<NexusModsUserMetadataEntity>().AsNoTracking().Prepare().FirstOrDefaultAsync(x => x.NexusModsUserId == validateResponse.UserId, ct);
        var metadata = metadataEntity?.Metadata ?? new();
        var existingMetadata = HttpContext.GetMetadata();
        var isMetadataEqual = metadata.Count == existingMetadata.Count && metadata.All(
            d1KV => existingMetadata.TryGetValue(d1KV.Key, out var d2Value) && (d1KV.Value == d2Value || d1KV.Value?.Equals(d2Value) == true));

        if (role != HttpContext.GetRole() || !isMetadataEqual)
        {
            var generatedToken = await _tokenGenerator.GenerateTokenAsync(new ButrNexusModsUserInfo
            {
                UserId = validateResponse.UserId,
                Name = validateResponse.Name,
                EMail = validateResponse.Email,
                ProfileUrl = validateResponse.ProfileUrl,
                IsSupporter = validateResponse.IsSupporter,
                IsPremium = validateResponse.IsPremium,
                APIKey = validateResponse.Key,
                Role = role,
                Metadata = metadata
            });
            return APIResponse(new JwtTokenResponse(generatedToken.Token, HttpContext.GetProfile(role)));
        }

        var token = Request.Headers["Authorization"].ToString().Replace(ButrNexusModsAuthSchemeConstants.AuthScheme, "").Trim();
        return APIResponse(new JwtTokenResponse(token, HttpContext.GetProfile(HttpContext.GetRole())));
    }
}