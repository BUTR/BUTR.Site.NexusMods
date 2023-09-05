using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
public sealed class DiscordController : ControllerExtended
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
    public ActionResult<APIResponse<DiscordOAuthUrlModel?>> GetOAuthUrl()
    {
        var (url, state) = _discordClient.GetOAuthUrl();
        return APIResponse(new DiscordOAuthUrlModel(url, state));
    }

    [HttpGet("Link")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> LinkAsync([FromQuery] string code, CancellationToken ct)
    {
        var tokens = await _discordClient.CreateTokensAsync(code, ct);
        if (tokens is null)
            return APIResponseError<string>("Failed to link!");

        var userId = HttpContext.GetUserId();
        var userInfo = await _discordClient.GetUserInfoAsync(tokens, ct);

        if (userInfo is null || !await _discordStorage.UpsertAsync(userId, userInfo.User.Id, tokens))
            return APIResponseError<string>("Failed to link!");

        await UpdateMetadataInternalAsync(ct);

        return APIResponse("Linked successful!");
    }

    [HttpPost("Unlink")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> UnlinkAsync(CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        var tokens = HttpContext.GetDiscordTokens();

        if (tokens?.Data is null)
            return APIResponseError<string>("Unlinked successful!");

        var refreshed = await _discordClient.GetOrRefreshTokensAsync(tokens.Data, ct);
        if (refreshed is null)
            return APIResponseError<string>("Failed to unlink!");

        if (tokens.Data.AccessToken != refreshed.AccessToken)
            await _discordStorage.UpsertAsync(userId, tokens.ExternalId, refreshed);

        if (!await _discordClient.PushMetadataAsync(refreshed, new Metadata(0, 0, 0, 0), ct))
            return APIResponseError<string>("Failed to unlink!");

        if (!await _discordStorage.RemoveAsync(userId, tokens.ExternalId))
            return APIResponseError<string>("Failed to unlink!");

        return APIResponse("Unlinked successful!");
    }

    [HttpPost("UpdateMetadata")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> UpdateMetadataAsync(CancellationToken ct)
    {
        if (await UpdateMetadataInternalAsync(ct) is not { } result)
            return APIResponseError<string>("Failed to update");
        return result ? APIResponse("") : APIResponseError<string>("Failed to update");
    }


    [HttpPost("GetUserInfo")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<DiscordUserInfo?>>> GetUserInfoByAccessTokenAsync(CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        var tokens = HttpContext.GetDiscordTokens();

        if (tokens?.Data is null)
            return APIResponseError<DiscordUserInfo>("Failed to get the token!");

        var refreshed = await _discordClient.GetOrRefreshTokensAsync(tokens.Data, ct);
        if (refreshed is null)
            return APIResponse<DiscordUserInfo>(null);

        if (tokens.Data.AccessToken != refreshed.AccessToken)
            await _discordStorage.UpsertAsync(userId, tokens.ExternalId, refreshed);

        var result = await _discordClient.GetUserInfoAsync(refreshed, ct);
        return APIResponse(result);
    }

    private async Task<bool?> UpdateMetadataInternalAsync(CancellationToken ct)
    {
        var role = HttpContext.GetRole();
        var userId = HttpContext.GetUserId();
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