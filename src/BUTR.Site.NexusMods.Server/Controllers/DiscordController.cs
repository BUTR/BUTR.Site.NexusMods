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
public sealed class DiscordController : ApiControllerBase
{
    public sealed record DiscordOAuthUrlModel(string Url, Guid State);

    private sealed record Metadata(
        [property: JsonPropertyName(DiscordConstants.BUTRModAuthor)] int IsModAuthor,
        [property: JsonPropertyName(DiscordConstants.BUTRModerator)] int IsModerator,
        [property: JsonPropertyName(DiscordConstants.BUTRAdministrator)] int IsAdministrator,
        [property: JsonPropertyName(DiscordConstants.BUTRLinkedMods)] int LinkedMods);

    private readonly DiscordClient _discordClient;
    private readonly IDiscordStorage _discordStorage;
    private readonly IAppDbContextRead _dbContextRead;

    public DiscordController(DiscordClient discordClient, IDiscordStorage discordStorage, IAppDbContextRead dbContextRead)
    {
        _discordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
        _discordStorage = discordStorage ?? throw new ArgumentNullException(nameof(discordStorage));
        _dbContextRead = dbContextRead ?? throw new ArgumentNullException(nameof(dbContextRead));
    }

    [HttpGet("GetOAuthUrl")]
    [Produces("application/json")]
    public ApiResult<DiscordOAuthUrlModel?> GetOAuthUrl()
    {
        var (url, state) = _discordClient.GetOAuthUrl();
        return ApiResult(new DiscordOAuthUrlModel(url, state));
    }

    [HttpGet("Link")]
    public async Task<ApiResult<string?>> LinkAsync([FromQuery] string code, [BindRole] ApplicationRole role, [BindUserId] NexusModsUserId userId, CancellationToken ct)
    {
        var tokens = await _discordClient.CreateTokensAsync(code, ct);
        if (tokens is null)
            return ApiBadRequest("Failed to link!");

        var userInfo = await _discordClient.GetUserInfoAsync(tokens, ct);

        if (userInfo is null || !await _discordStorage.UpsertAsync(userId, userInfo.User.Id, tokens))
            return ApiBadRequest("Failed to link!");

        await UpdateMetadataInternalAsync(role, userId, ct);

        return ApiResult("Linked successful!");
    }

    [HttpPost("Unlink")]
    public async Task<ApiResult<string?>> UnlinkAsync([BindUserId] NexusModsUserId userId, CancellationToken ct)
    {
        var tokens = HttpContext.GetDiscordTokens();

        if (tokens?.Data is null)
            return ApiBadRequest("Unlinked successful!");

        var refreshed = await _discordClient.GetOrRefreshTokensAsync(tokens.Data, ct);
        if (refreshed is null)
            return ApiBadRequest("Failed to unlink!");

        if (tokens.Data.AccessToken != refreshed.AccessToken)
            await _discordStorage.UpsertAsync(userId, tokens.ExternalId, refreshed);

        if (!await _discordClient.PushMetadataAsync(refreshed, new Metadata(0, 0, 0, 0), ct))
            return ApiBadRequest("Failed to unlink!");

        if (!await _discordStorage.RemoveAsync(userId, tokens.ExternalId))
            return ApiBadRequest("Failed to unlink!");

        return ApiResult("Unlinked successful!");
    }

    [HttpPost("UpdateMetadata")]
    public async Task<ApiResult<string?>> UpdateMetadataAsync([BindRole] ApplicationRole role, [BindUserId] NexusModsUserId userId, CancellationToken ct)
    {
        if (await UpdateMetadataInternalAsync(role, userId, ct) is not { } result)
            return ApiBadRequest("Failed to update");
        return result ? ApiResult("") : ApiBadRequest("Failed to update");
    }


    [HttpPost("GetUserInfo")]
    public async Task<ApiResult<DiscordUserInfo?>> GetUserInfoByAccessTokenAsync([BindUserId] NexusModsUserId userId, CancellationToken ct)
    {
        var tokens = HttpContext.GetDiscordTokens();

        if (tokens?.Data is null)
            return ApiBadRequest("Failed to get the token!");

        var refreshed = await _discordClient.GetOrRefreshTokensAsync(tokens.Data, ct);
        if (refreshed is null)
            return ApiResult<DiscordUserInfo?>(null);

        if (tokens.Data.AccessToken != refreshed.AccessToken)
            await _discordStorage.UpsertAsync(userId, tokens.ExternalId, refreshed);

        var result = await _discordClient.GetUserInfoAsync(refreshed, ct);
        return ApiResult(result);
    }

    private async Task<bool?> UpdateMetadataInternalAsync(ApplicationRole role, NexusModsUserId userId, CancellationToken ct)
    {
        var tokens = HttpContext.GetDiscordTokens();

        if (tokens?.Data is null)
            return null;

        var linkedModsCount = await _dbContextRead.NexusModsUsers
            .Include(x => x.ToNexusModsMods)
            .AsSplitQuery()
            .Where(x => x.NexusModsUserId == userId)
            .SelectMany(x => x.ToNexusModsMods)
            .CountAsync(ct);

        var refreshed = await _discordClient.GetOrRefreshTokensAsync(tokens.Data, ct);
        if (refreshed is null)
            return false;

        if (tokens.Data.AccessToken != refreshed.AccessToken)
            await _discordStorage.UpsertAsync(userId, tokens.ExternalId, refreshed);

        return await _discordClient.PushMetadataAsync(refreshed, new Metadata(
            1,
            role == ApplicationRole.Moderator ? 1 : 0,
            role == ApplicationRole.Administrator ? 1 : 0,
            linkedModsCount), ct);
    }
}