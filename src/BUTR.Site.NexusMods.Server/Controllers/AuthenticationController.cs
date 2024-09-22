using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Authentication.NexusMods.Services;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Repositories;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.BindingSources;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), ButrNexusModsAuthorization]
public sealed class AuthenticationController : ApiControllerBase
{
    public sealed record NexusModsOAuthUrlModel(string Url, Guid State);


    private readonly ILogger _logger;
    private readonly INexusModsAPIClient _nexusModsAPIClient;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly INexusModsUsersClient _nexusModsUsersClient;
    private readonly IMemoryCache _memoryCache;
    public AuthenticationController(
        ILogger<AuthenticationController> logger,
        INexusModsAPIClient nexusModsAPIClient,
        IUnitOfWorkFactory unitOfWorkFactory,
        ITokenGenerator tokenGenerator,
        IOptions<JsonSerializerOptions> jsonSerializerOptions, INexusModsUsersClient nexusModsUsersClient, IMemoryCache memoryCache)
    {
        _logger = logger;
        _nexusModsAPIClient = nexusModsAPIClient;
        _unitOfWorkFactory = unitOfWorkFactory;
        _tokenGenerator = tokenGenerator;
        _nexusModsUsersClient = nexusModsUsersClient;
        _memoryCache = memoryCache;
        _jsonSerializerOptions = jsonSerializerOptions.Value;
    }

    [HttpPost("Authenticate"), AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<JwtTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<JwtTokenResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ApiResult<JwtTokenResponse?>> AuthenticateWithApiKeyAsync([FromHeader, Required] NexusModsApiKey apiKey, [BindTenant] TenantId tenant, CancellationToken ct)
    {
        if (await _nexusModsAPIClient.ValidateAPIKeyAsync(apiKey, ct) is not { } validateResponse)
            return ApiResultError("Invalid apiKey!", StatusCodes.Status401Unauthorized);

        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var userEntity = await unitOfRead.NexusModsUsers.GetUserWithIntegrationsAsync(validateResponse.UserId, ct);
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
            AccessToken = null,
            RefreshToken = null,
            Role = role.Value,
            Metadata = metadata
        });
        return ApiResult(new JwtTokenResponse(generatedToken.Token, HttpContext.GetProfile(validateResponse, role, metadata)));
    }

    [HttpPost("Validate"), AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<JwtTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<JwtTokenResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ApiResult<JwtTokenResponse?>> ValidateAsync([BindRole] ApplicationRole role, [BindTenant] TenantId tenant, CancellationToken ct)
    {
        if (HttpContext.GetAPIKey() is { } apiKey && apiKey != NexusModsApiKey.None)
            return await ValidateAPIKeyAsync(apiKey, role, tenant, ct);

        if (HttpContext.GetTokens() is { } tokens)
            return await ValidateTokenAsync(tokens, role, tenant, ct);

        return ApiResultError("", StatusCodes.Status401Unauthorized);
    }

    private async Task<ApiResult<JwtTokenResponse?>> ValidateAPIKeyAsync(NexusModsApiKey apiKey, ApplicationRole role, TenantId tenant, CancellationToken ct)
    {
        if (await _nexusModsAPIClient.ValidateAPIKeyAsync(apiKey, ct) is not { } validateResponse)
            return ApiResultError("API Key not valid", StatusCodes.Status401Unauthorized);

        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var userEntity = await unitOfRead.NexusModsUsers.GetUserWithIntegrationsAsync(validateResponse.UserId, ct);
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
                AccessToken = null,
                RefreshToken = null,
                Role = userRole.Value,
                Metadata = metadata,
            });
            return ApiResult(new JwtTokenResponse(generatedToken.Token, HttpContext.GetProfile(validateResponse, userRole, metadata)));
        }

        var authenticationHeaderValue = AuthenticationHeaderValue.Parse(Request.Headers.Authorization!);
        var token = authenticationHeaderValue.Parameter!;
        return ApiResult(new JwtTokenResponse(token, HttpContext.GetProfile()));
    }

    [AllowAnonymous]
    [HttpGet("OAuthUrl")]
    public ApiResult<NexusModsOAuthUrlModel?> GetOAuthUrl()
    {
        var (url, codeVerifier, state) = _nexusModsUsersClient.GetOAuthUrl();
        _memoryCache.Set<string>($"NMOAUTH-{state}", codeVerifier, TimeSpan.FromMinutes(10));
        return ApiResult(new NexusModsOAuthUrlModel(url, state));
    }

    [AllowAnonymous]
    [HttpGet("AuthenticateCallback")]
    public async Task<ApiResult<JwtTokenResponse?>> AuthenticateWithOAuth2Async([FromQuery] string code, [FromQuery] Guid state, [BindTenant] TenantId tenant, CancellationToken ct)
    {
        var codeVerifier = _memoryCache.Get<string>($"NMOAUTH-{state}");
        if (string.IsNullOrWhiteSpace(codeVerifier))
            return ApiBadRequest("Invalid state");

        if (await _nexusModsUsersClient.CreateTokensAsync(code, codeVerifier, ct) is not { } tokens)
            return ApiResultError("Invalid code!", StatusCodes.Status401Unauthorized);

        if (await _nexusModsUsersClient.GetUserInfoAsync(tokens, ct) is not { } userInfo)
            return ApiResultError("Failed to get User Info!", StatusCodes.Status401Unauthorized);

        if (!NexusModsUserId.TryParse(userInfo.UserId, out var nexusModsUserId))
            return ApiResultError("Unvalid User Id!", StatusCodes.Status401Unauthorized);

        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var userEntity = await unitOfRead.NexusModsUsers.GetUserWithIntegrationsAsync(nexusModsUserId, ct);
        var role = userEntity?.ToRoles.FirstOrDefault(x => x.TenantId == tenant)?.Role ?? ApplicationRole.User;

        var typedMetadata = UserTypedMetadata.FromUser(userEntity);
        var metadata = new Dictionary<string, string>
        {
            { nameof(UserTypedMetadata), JsonSerializer.Serialize(typedMetadata, _jsonSerializerOptions) }
        };
        var generatedToken = await _tokenGenerator.GenerateTokenAsync(new ButrNexusModsUserInfo
        {
            UserId = (uint) nexusModsUserId.Value,
            Name = userInfo.Name.Value,
            EMail = userInfo.Email.Value,
            ProfileUrl = userInfo.AvatarUrl ?? "",
            IsSupporter = userInfo.MembershipRoles.Contains("supporter"),
            IsPremium = userInfo.MembershipRoles.Contains("premium"),
            APIKey = null,
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            Role = role.Value,
            Metadata = metadata
        });
        return ApiResult(new JwtTokenResponse(generatedToken.Token, HttpContext.GetProfile(userInfo, role, metadata)));
    }

    private async Task<ApiResult<JwtTokenResponse?>> ValidateTokenAsync(NexusModsOAuthTokens tokens, ApplicationRole role, TenantId tenant, CancellationToken ct)
    {
        if (await _nexusModsUsersClient.GetUserInfoAsync(tokens, ct) is not { } userInfo)
            return ApiResultError("Failed to get User Info!", StatusCodes.Status401Unauthorized);

        if (!NexusModsUserId.TryParse(userInfo.UserId, out var nexusModsUserId))
            return ApiResultError("Unvalid User Id!", StatusCodes.Status401Unauthorized);

        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var userEntity = await unitOfRead.NexusModsUsers.GetUserWithIntegrationsAsync(nexusModsUserId, ct);
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
                UserId = (uint) nexusModsUserId.Value,
                Name = userInfo.Name.Value,
                EMail = userInfo.Email.Value,
                ProfileUrl = $"https://www.nexusmods.com/users/{userInfo.UserId}",
                IsSupporter = userInfo.MembershipRoles.Contains("supporter"),
                IsPremium = userInfo.MembershipRoles.Contains("premium"),
                APIKey = null,
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                Role = role.Value,
                Metadata = metadata,
            });
            return ApiResult(new JwtTokenResponse(generatedToken.Token, HttpContext.GetProfile(userInfo, userRole, metadata)));
        }

        var authenticationHeaderValue = AuthenticationHeaderValue.Parse(Request.Headers.Authorization!);
        var token = authenticationHeaderValue.Parameter!;
        return ApiResult(new JwtTokenResponse(token, HttpContext.GetProfile()));
    }
}