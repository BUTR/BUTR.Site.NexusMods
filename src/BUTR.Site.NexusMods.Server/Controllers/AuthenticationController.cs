using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Authentication.NexusMods.Services;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
public sealed class AuthenticationController : ControllerExtended
{
    private readonly ILogger _logger;
    private readonly NexusModsAPIClient _nexusModsAPIClient;
    private readonly IAppDbContextRead _dbContextRead;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public AuthenticationController(
        ILogger<AuthenticationController> logger,
        NexusModsAPIClient nexusModsAPIClient,
        IAppDbContextRead dbContextRead,
        ITokenGenerator tokenGenerator,
        IOptions<JsonSerializerOptions> jsonSerializerOptions)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _nexusModsAPIClient = nexusModsAPIClient ?? throw new ArgumentNullException(nameof(nexusModsAPIClient));
        _dbContextRead = dbContextRead ?? throw new ArgumentNullException(nameof(dbContextRead));
        _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
        _jsonSerializerOptions = jsonSerializerOptions.Value ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
    }

    [HttpGet("Authenticate"), AllowAnonymous]
    [Produces("application/json")]
    [ProducesResponseType(typeof(APIResponse<JwtTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<APIResponse<JwtTokenResponse?>>> AuthenticateAsync([FromHeader] string? apiKey, CancellationToken ct)
    {
        var tenant = HttpContext.GetTenant();
        if (tenant is null)
            return StatusCode(StatusCodes.Status400BadRequest);

        if (apiKey is null || await _nexusModsAPIClient.ValidateAPIKeyAsync(apiKey, ct) is not { } validateResponse)
            return StatusCode(StatusCodes.Status401Unauthorized);

        var userEntity = await _dbContextRead.NexusModsUsers
            .Include(x => x.ToRoles)
            .Include(x => x.ToDiscord!)
            .ThenInclude(x => x.ToTokens)
            .Include(x => x.ToGOG!)
            .ThenInclude(x => x.ToTokens)
            .Include(x => x.ToGOG!)
            .ThenInclude(x => x.ToOwnedTenants)
            .Include(x => x.ToSteam!)
            .ThenInclude(x => x.ToTokens)
            .Include(x => x.ToSteam!)
            .ThenInclude(x => x.ToOwnedTenants)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.NexusModsUserId == validateResponse.UserId, ct);
        var role = userEntity?.ToRoles.FirstOrDefault(x => x.TenantId == tenant.Value)?.Role ?? ApplicationRoles.User;

        var typedMetadata = UserTypedMetadata.FromUser(userEntity);
        var metadata = new Dictionary<string, string>
        {
            { nameof(UserTypedMetadata), JsonSerializer.Serialize(typedMetadata, _jsonSerializerOptions) }
        };
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
        return APIResponse(new JwtTokenResponse(generatedToken.Token, HttpContext.GetProfile(role, metadata)));
    }

    [HttpGet("Validate"), AllowAnonymous]
    [Produces("application/json")]
    [ProducesResponseType(typeof(APIResponse<JwtTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<APIResponse<JwtTokenResponse?>>> ValidateAsync(CancellationToken ct)
    {
        var tenant = HttpContext.GetTenant();
        if (tenant is null)
            return StatusCode(StatusCodes.Status400BadRequest);

        if (HttpContext.GetAPIKey() is not { } apikey || string.IsNullOrEmpty(apikey) || await _nexusModsAPIClient.ValidateAPIKeyAsync(apikey, ct) is not { } validateResponse)
            return StatusCode(StatusCodes.Status401Unauthorized);

        var userEntity = await _dbContextRead.NexusModsUsers
            .Include(x => x.ToRoles)
            .Include(x => x.ToDiscord!)
            .ThenInclude(x => x.ToTokens)
            .Include(x => x.ToGOG!)
            .ThenInclude(x => x.ToTokens)
            .Include(x => x.ToGOG!)
            .ThenInclude(x => x.ToOwnedTenants)
            .Include(x => x.ToSteam!)
            .ThenInclude(x => x.ToTokens)
            .Include(x => x.ToSteam!)
            .ThenInclude(x => x.ToOwnedTenants)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.NexusModsUserId == validateResponse.UserId, ct);

        var role = userEntity?.ToRoles.FirstOrDefault(x => x.TenantId == tenant)?.Role ?? ApplicationRoles.User;

        var typedMetadata = UserTypedMetadata.FromUser(userEntity);
        var metadata = new Dictionary<string, string>
        {
            { nameof(UserTypedMetadata), JsonSerializer.Serialize(typedMetadata, _jsonSerializerOptions) }
        };
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
            return APIResponse(new JwtTokenResponse(generatedToken.Token, HttpContext.GetProfile(role, metadata)));
        }

        var token = Request.Headers["Authorization"].ToString().Replace(ButrNexusModsAuthSchemeConstants.AuthScheme, "").Trim();
        return APIResponse(new JwtTokenResponse(token, HttpContext.GetProfile()));
    }
}