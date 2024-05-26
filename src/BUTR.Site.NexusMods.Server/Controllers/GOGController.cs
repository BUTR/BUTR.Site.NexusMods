using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

using Microsoft.AspNetCore.Mvc;

using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), ButrNexusModsAuthorization, TenantNotRequired]
public sealed class GOGController : ApiControllerBase
{
    public sealed record GOGOAuthUrlModel(string Url);

    private readonly IGOGStorage _gogStorage;
    private readonly IGOGAuthClient _gogAuthClient;
    private readonly IGOGEmbedClient _gogEmbedClient;

    public GOGController(IGOGStorage gogStorage, IGOGAuthClient gogAuthClient, IGOGEmbedClient gogEmbedClient)
    {
        _gogStorage = gogStorage;
        _gogAuthClient = gogAuthClient;
        _gogEmbedClient = gogEmbedClient;
    }

    [HttpPost]
    public async Task<ApiResult<string?>> AddLinkAsync([FromQuery, Required] string code, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();

        var tokens = await _gogAuthClient.CreateTokensAsync(code, ct);
        if (tokens is null)
            return ApiBadRequest("Failed to link!");

        if (!await _gogStorage.CheckOwnedGamesAsync(userId, tokens.UserId, tokens))
            return ApiBadRequest("Failed to link!");

        if (!await _gogStorage.UpsertAsync(userId, tokens.UserId, tokens))
            return ApiBadRequest("Failed to link!");

        return ApiResult("Linked successful!");
    }

    [HttpDelete]
    public async Task<ApiResult<string?>> RemoveLinkAsync()
    {
        var userId = HttpContext.GetUserId();

        var tokens = HttpContext.GetGOGTokens();

        if (tokens?.Data is null)
            return ApiBadRequest("Unlinked successful!");

        if (!await _gogStorage.RemoveAsync(userId, tokens.ExternalId))
            return ApiBadRequest("Failed to unlink!");

        return ApiResult("Unlinked successful!");
    }

    [HttpGet("OAuthUrl")]
    public ApiResult<GOGOAuthUrlModel?> GetOAuthUrl()
    {
        return ApiResult(new GOGOAuthUrlModel(_gogAuthClient.GetOAuth2Url()));
    }

    [HttpGet("UserInfo")]
    public async Task<ApiResult<GOGUserInfo?>> GetUserInfoAsync(CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();

        var tokens = HttpContext.GetGOGTokens();

        if (tokens?.Data is null)
            return ApiBadRequest("Failed to get the token!");

        var refreshed = await _gogAuthClient.GetOrRefreshTokensAsync(tokens.Data, ct);
        if (refreshed is null)
            return ApiResult<GOGUserInfo?>(null);

        if (tokens.Data.AccessToken != refreshed.AccessToken)
            await _gogStorage.UpsertAsync(userId, refreshed.UserId, refreshed);

        var result = await _gogEmbedClient.GetUserInfoAsync(refreshed.AccessToken, ct);
        return ApiResult(result);
    }
}