using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.BindingSources;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

using Microsoft.AspNetCore.Mvc;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), ButrNexusModsAuthorization, TenantNotRequired]
public sealed class GOGController : ApiControllerBase
{
    public sealed record GOGOAuthUrlModel(string Url);


    private readonly IGOGStorage _gogStorage;
    private readonly GOGAuthClient _gogAuthClient;
    private readonly GOGEmbedClient _gogEmbedClient;

    public GOGController(IGOGStorage gogStorage, GOGAuthClient gogAuthClient, GOGEmbedClient gogEmbedClient)
    {
        _gogStorage = gogStorage ?? throw new ArgumentNullException(nameof(gogStorage));
        _gogAuthClient = gogAuthClient ?? throw new ArgumentNullException(nameof(gogAuthClient));
        _gogEmbedClient = gogEmbedClient ?? throw new ArgumentNullException(nameof(gogEmbedClient));
    }

    [HttpGet("GetOAuthUrl")]
    public ApiResult<GOGOAuthUrlModel?> GetOpenIdUrl()
    {
        return ApiResult(new GOGOAuthUrlModel(_gogAuthClient.GetOAuth2Url()));
    }

    [HttpGet("Link")]
    public async Task<ApiResult<string?>> LinkAsync([FromQuery] string code, [BindUserId] NexusModsUserId userId, CancellationToken ct)
    {
        var tokens = await _gogAuthClient.CreateTokensAsync(code, ct);
        if (tokens is null)
            return ApiResultError("Failed to link!");

        if (!await _gogStorage.CheckOwnedGamesAsync(userId, tokens.UserId, tokens))
            return ApiResultError("Failed to link!");

        if (!await _gogStorage.UpsertAsync(userId, tokens.UserId, tokens))
            return ApiResultError("Failed to link!");

        return ApiResult("Linked successful!");
    }

    [HttpPost("Unlink")]
    public async Task<ApiResult<string?>> UnlinkAsync([BindUserId] NexusModsUserId userId)
    {
        var tokens = HttpContext.GetGOGTokens();

        if (tokens?.Data is null)
            return ApiResultError("Unlinked successful!");

        if (!await _gogStorage.RemoveAsync(userId, tokens.ExternalId))
            return ApiResultError("Failed to unlink!");

        return ApiResult("Unlinked successful!");
    }

    [HttpPost("GetUserInfo")]
    public async Task<ApiResult<GOGUserInfo?>> GetUserInfoByAccessTokenAsync([BindUserId] NexusModsUserId userId, CancellationToken ct)
    {
        var tokens = HttpContext.GetGOGTokens();

        if (tokens?.Data is null)
            return ApiResultError("Failed to get the token!");

        var refreshed = await _gogAuthClient.GetOrRefreshTokensAsync(tokens.Data, ct);
        if (refreshed is null)
            return ApiResult<GOGUserInfo>(null);

        if (tokens.Data.AccessToken != refreshed.AccessToken)
            await _gogStorage.UpsertAsync(userId, refreshed.UserId, refreshed);

        var result = await _gogEmbedClient.GetUserInfoAsync(refreshed.AccessToken, ct);
        return ApiResult(result);
    }
}