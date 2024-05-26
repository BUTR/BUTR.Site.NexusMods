using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

using Microsoft.AspNetCore.Mvc;

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), ButrNexusModsAuthorization, TenantNotRequired]
public sealed class GitHubController : ApiControllerBase
{
    public sealed record GitHubOAuthUrlModel(string Url, Guid State);

    private readonly IGitHubClient _gitHubClient;
    private readonly IGitHubAPIClient _gitHubApiClient;
    private readonly IGitHubStorage _gitHubStorage;

    public GitHubController(IGitHubClient gitHubClient, IGitHubAPIClient gitHubApiClient, IGitHubStorage gitHubStorage)
    {
        _gitHubClient = gitHubClient;
        _gitHubApiClient = gitHubApiClient;
        _gitHubStorage = gitHubStorage;
    }

    [HttpPost]
    public async Task<ApiResult<string?>> AddLinkAsync([FromQuery, Required] string code, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();

        var tokens = await _gitHubClient.CreateTokensAsync(code, ct);
        if (tokens is null)
            return ApiBadRequest("Failed to link!");

        var userInfo = await _gitHubApiClient.GetUserInfoAsync(tokens, ct);

        if (userInfo is null || !await _gitHubStorage.UpsertAsync(userId, userInfo.Id.ToString(), tokens))
            return ApiBadRequest("Failed to link!");

        return ApiResult("Linked successful!");
    }

    [HttpDelete]
    public async Task<ApiResult<string?>> RemoveLinkAsync(CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();

        var tokens = HttpContext.GetDiscordTokens();

        if (tokens?.Data is null)
            return ApiBadRequest("Unlinked successful!");

        //var refreshed = await _gitHubClient.GetOrRefreshTokensAsync(tokens.Data, ct);
        //if (refreshed is null)
        //    return ApiBadRequest("Failed to unlink!");

        //if (tokens.Data.AccessToken != refreshed.AccessToken)
        //    await _discordStorage.UpsertAsync(userId, tokens.ExternalId, refreshed);

        if (!await _gitHubStorage.RemoveAsync(userId, tokens.ExternalId))
            return ApiBadRequest("Failed to unlink!");

        return ApiResult("Unlinked successful!");
    }

    [HttpGet("OAuthUrl")]
    public ApiResult<GitHubOAuthUrlModel?> GetOAuthUrl()
    {
        var (url, state) = _gitHubClient.GetOAuthUrl();
        return ApiResult(new GitHubOAuthUrlModel(url, state));
    }

    [HttpGet("UserInfo")]
    public async Task<ApiResult<GitHubUserInfo?>> GetUserInfoAsync(CancellationToken ct)
    {
        var tokens = HttpContext.GetGitHubTokens();

        if (tokens?.Data is null)
            return ApiBadRequest("Failed to get the token!");

        //var refreshed = await _gitHubClient.GetOrRefreshTokensAsync(tokens.Data, ct);
        //if (refreshed is null)
        //    return ApiResult<DiscordUserInfo?>(null);

        //if (tokens.Data.AccessToken != refreshed.AccessToken)
        //    await _discordStorage.UpsertAsync(userId, tokens.ExternalId, refreshed);

        //var result = await _gitHubApiClient.GetUserInfoAsync(refreshed, ct);
        var result = await _gitHubApiClient.GetUserInfoAsync(tokens.Data, ct);
        return ApiResult(result);
    }
}