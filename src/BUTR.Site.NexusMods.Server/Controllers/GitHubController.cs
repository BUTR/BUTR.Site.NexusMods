using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.BindingSources;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), ButrNexusModsAuthorization, TenantNotRequired]
public sealed class GitHubController : ApiControllerBase
{
    public sealed record GitHubOAuthUrlModel(string Url, Guid State);

    private readonly GitHubClient _gitHubClient;
    private readonly GitHubAPIClient _gitHubApiClient;
    private readonly IGitHubStorage _gitHubStorage;

    public GitHubController(GitHubClient gitHubClient, GitHubAPIClient gitHubApiClient, IGitHubStorage gitHubStorage)
    {
        _gitHubClient = gitHubClient ?? throw new ArgumentNullException(nameof(gitHubClient));
        _gitHubApiClient = gitHubApiClient ?? throw new ArgumentNullException(nameof(gitHubApiClient));
        _gitHubStorage = gitHubStorage ?? throw new ArgumentNullException(nameof(gitHubStorage));
    }

    [HttpGet("GetOAuthUrl")]
    [Produces("application/json")]
    public ApiResult<GitHubOAuthUrlModel?> GetOAuthUrl()
    {
        var (url, state) = _gitHubClient.GetOAuthUrl();
        return ApiResult(new GitHubOAuthUrlModel(url, state));
    }

    [HttpGet("Link")]
    public async Task<ApiResult<string?>> LinkAsync([FromQuery] string code, [BindRole] ApplicationRole role, [BindUserId] NexusModsUserId userId, CancellationToken ct)
    {
        var tokens = await _gitHubClient.CreateTokensAsync(code, ct);
        if (tokens is null)
            return ApiBadRequest("Failed to link!");

        var userInfo = await _gitHubApiClient.GetUserInfoAsync(tokens, ct);

        if (userInfo is null || !await _gitHubStorage.UpsertAsync(userId, userInfo.Id.ToString(), tokens))
            return ApiBadRequest("Failed to link!");

        return ApiResult("Linked successful!");
    }

    [HttpPost("Unlink")]
    public async Task<ApiResult<string?>> UnlinkAsync([BindUserId] NexusModsUserId userId, CancellationToken ct)
    {
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

    [HttpPost("GetUserInfo")]
    public async Task<ApiResult<GitHubUserInfo?>> GetUserInfoByAccessTokenAsync([BindUserId] NexusModsUserId userId, CancellationToken ct)
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