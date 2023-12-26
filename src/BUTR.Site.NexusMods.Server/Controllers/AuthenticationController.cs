using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Authentication.NexusMods.Services;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.BindingSources;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), ButrNexusModsAuthorization]
public sealed class AuthenticationController : ApiControllerBase
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
    [ProducesResponseType(typeof(ApiResult<JwtTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<JwtTokenResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ApiResult<JwtTokenResponse?>> AuthenticateAsync([Required, FromHeader] NexusModsApiKey apiKey, [BindTenant] TenantId tenant, CancellationToken ct)
    {
        if (await _nexusModsAPIClient.ValidateAPIKeyAsync(apiKey, ct) is not { } validateResponse)
            return ApiResultError("Invalid apiKey!", StatusCodes.Status401Unauthorized);

        var userEntity = await _dbContextRead.NexusModsUsers
            .Include(x => x.ToRoles)
            .Include(x => x.ToGitHub!).ThenInclude(x => x.ToTokens)
            .Include(x => x.ToDiscord!).ThenInclude(x => x.ToTokens)
            .Include(x => x.ToGOG!).ThenInclude(x => x.ToTokens)
            .Include(x => x.ToGOG!).ThenInclude(x => x.ToOwnedTenants)
            .Include(x => x.ToSteam!).ThenInclude(x => x.ToTokens)
            .Include(x => x.ToSteam!).ThenInclude(x => x.ToOwnedTenants)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.NexusModsUserId == validateResponse.UserId, ct);
        var role = userEntity?.ToRoles.FirstOrDefault(x => x.TenantId == tenant)?.Role ?? ApplicationRole.User;

        var typedMetadata = UserTypedMetadata.FromUser(userEntity);
        var metadata = new Dictionary<string, string>
        {
            { nameof(UserTypedMetadata), JsonSerializer.Serialize(typedMetadata, _jsonSerializerOptions) }
        };
        var generatedToken = await _tokenGenerator.GenerateTokenAsync(new ButrNexusModsUserInfo
        {
            UserId = (uint) validateResponse.UserId.Value,
            Name = validateResponse.Name.Value,
            EMail = validateResponse.Email.Value,
            ProfileUrl = validateResponse.ProfileUrl,
            IsSupporter = validateResponse.IsSupporter,
            IsPremium = validateResponse.IsPremium,
            APIKey = validateResponse.Key.Value,
            Role = role.Value,
            Metadata = metadata
        });
        return ApiResult(new JwtTokenResponse(generatedToken.Token, HttpContext.GetProfile(validateResponse, role, metadata)));
    }

    [HttpGet("Validate"), AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<JwtTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<JwtTokenResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ApiResult<JwtTokenResponse?>> ValidateAsync([BindApiKey] NexusModsApiKey apiKey, [BindRole] ApplicationRole role, [BindTenant] TenantId tenant, CancellationToken ct)
    {
        if (await _nexusModsAPIClient.ValidateAPIKeyAsync(apiKey, ct) is not { } validateResponse)
            return ApiResultError("API Key not valid", StatusCodes.Status401Unauthorized);

        var userEntity = await _dbContextRead.NexusModsUsers
            .Include(x => x.ToRoles)
            .Include(x => x.ToGitHub!).ThenInclude(x => x.ToTokens)
            .Include(x => x.ToDiscord!).ThenInclude(x => x.ToTokens)
            .Include(x => x.ToGOG!).ThenInclude(x => x.ToTokens)
            .Include(x => x.ToGOG!).ThenInclude(x => x.ToOwnedTenants)
            .Include(x => x.ToSteam!).ThenInclude(x => x.ToTokens)
            .Include(x => x.ToSteam!).ThenInclude(x => x.ToOwnedTenants)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.NexusModsUserId == validateResponse.UserId, ct);

        var userRole = userEntity?.ToRoles.FirstOrDefault(x => x.TenantId == tenant)?.Role ?? ApplicationRole.User;

        var typedMetadata = UserTypedMetadata.FromUser(userEntity);
        var metadata = new Dictionary<string, string>
        {
            { nameof(UserTypedMetadata), JsonSerializer.Serialize(typedMetadata, _jsonSerializerOptions) }
        };
        var existingMetadata = HttpContext.GetMetadata();
        var isMetadataEqual = metadata.Count == existingMetadata.Count && metadata.All(
            d1KV => existingMetadata.TryGetValue(d1KV.Key, out var d2Value) && (d1KV.Value == d2Value || d1KV.Value.Equals(d2Value)));

        if (userRole != role || !isMetadataEqual)
        {
            var generatedToken = await _tokenGenerator.GenerateTokenAsync(new ButrNexusModsUserInfo
            {
                UserId = (uint) validateResponse.UserId.Value,
                Name = validateResponse.Name.Value,
                EMail = validateResponse.Email.Value,
                ProfileUrl = validateResponse.ProfileUrl,
                IsSupporter = validateResponse.IsSupporter,
                IsPremium = validateResponse.IsPremium,
                APIKey = validateResponse.Key.Value,
                Role = userRole.Value,
                Metadata = metadata
            });
            return ApiResult(new JwtTokenResponse(generatedToken.Token, HttpContext.GetProfile(validateResponse, userRole, metadata)));
        }

        var token = Request.Headers["Authorization"].ToString().Replace(ButrNexusModsAuthSchemeConstants.AuthScheme, "").Trim();
        return ApiResult(new JwtTokenResponse(token, HttpContext.GetProfile()));
    }
}