using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Repositories;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.BindingSources;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Mvc;

using System;
using System.ComponentModel.DataAnnotations;
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

    private readonly IDiscordClient _discordClient;
    private readonly IDiscordStorage _discordStorage;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public DiscordController(IDiscordClient discordClient, IDiscordStorage discordStorage, IUnitOfWorkFactory unitOfWorkFactory)
    {
        _discordClient = discordClient;
        _discordStorage = discordStorage;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    [HttpPost]
    public async Task<ApiResult<string?>> AddLinkAsync([FromQuery, Required] string code, [BindRole] ApplicationRole role, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();

        var tokens = await _discordClient.CreateTokensAsync(code, ct);
        if (tokens is null)
            return ApiBadRequest("Failed to link!");

        var userInfo = await _discordClient.GetUserInfoAsync(tokens, ct);

        if (userInfo is null || !await _discordStorage.UpsertAsync(userId, userInfo.User.Id, tokens))
            return ApiBadRequest("Failed to link!");

        await UpdateMetadataInternalAsync(role, userId, ct);

        return ApiResult("Linked successful!");
    }

    [HttpDelete]
    public async Task<ApiResult<string?>> RemoveLinkAsync(CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();

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

    [HttpGet("OAuthUrl")]
    public ApiResult<DiscordOAuthUrlModel?> GetOAuthUrl()
    {
        var (url, state) = _discordClient.GetOAuthUrl();
        return ApiResult(new DiscordOAuthUrlModel(url, state));
    }

    [HttpPut("Metadata")]
    public async Task<ApiResult<string?>> UpdateMetadataAsync([BindRole] ApplicationRole role, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();

        if (await UpdateMetadataInternalAsync(role, userId, ct) is not { } result)
            return ApiBadRequest("Failed to update");

        return result ? ApiResult("") : ApiBadRequest("Failed to update");
    }

    [HttpGet("UserInfo")]
    public async Task<ApiResult<DiscordUserInfo?>> GetUserInfoAsync(CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();

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

        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead(TenantId.None);

        var linkedModsCount = await unitOfRead.NexusModsUsers.GetLinkedNexusModsModCountAsync(userId, ct);

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