using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Options;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.BindingSources;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), ButrNexusModsAuthorization, TenantNotRequired]
public sealed class SteamController : ApiControllerBase
{
    public sealed record SteamOpenIdUrlModel(string Url);


    private readonly ISteamStorage _steamStorage;
    private readonly SteamAPIOptions _options;
    private readonly SteamCommunityClient _steamCommunityClient;
    private readonly SteamAPIClient _steamAPIClient;

    public SteamController(ISteamStorage steamStorage, IOptions<SteamAPIOptions> options, SteamCommunityClient steamCommunityClient, SteamAPIClient steamAPIClient)
    {
        _steamStorage = steamStorage ?? throw new ArgumentNullException(nameof(steamStorage));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _steamCommunityClient = steamCommunityClient ?? throw new ArgumentNullException(nameof(steamCommunityClient));
        _steamAPIClient = steamAPIClient ?? throw new ArgumentNullException(nameof(steamAPIClient));
    }

    [HttpGet("GetOpenIdUrl")]
    public ApiResult<SteamOpenIdUrlModel?> GetOpenIdUrl()
    {
        var query = QueryString.Create(new Dictionary<string, string?>
        {
            ["openid.ns"] = "http://specs.openid.net/auth/2.0",
            ["openid.mode"] = "checkid_setup",
            ["openid.return_to"] = _options.RedirectUri,
            ["openid.realm"] = _options.Realm,
            ["openid.identity"] = "http://specs.openid.net/auth/2.0/identifier_select",
            ["openid.claimed_id"] = "http://specs.openid.net/auth/2.0/identifier_select",
        });
        var steamLoginUrl = new UriBuilder("https://steamcommunity.com/openid/login") { Query = query.ToUriComponent() };

        return ApiResult(new SteamOpenIdUrlModel(steamLoginUrl.ToString()));
    }

    [HttpGet("Link")]
    public async Task<ApiResult<string?>> LinkAsync([FromQuery] Dictionary<string, string> queries, [BindUserId] NexusModsUserId userId, CancellationToken ct)
    {
        var isValid = await _steamCommunityClient.ConfirmIdentityAsync(queries, ct);
        if (!isValid)
            return ApiBadRequest("Failed to link!");

        if (!SteamUtils.TryParse(queries["openid.claimed_id"], out var steamId))
            return ApiBadRequest("Failed to link!");

        var userInfo = await _steamAPIClient.GetUserInfoAsync(steamId, ct);

        if (!await _steamStorage.CheckOwnedGamesAsync(userId, steamId))
            return ApiBadRequest("Failed to link!");

        if (userInfo is null || !await _steamStorage.UpsertAsync(userId, userInfo.Id, queries))
            return ApiBadRequest("Failed to link!");

        return ApiResult("Linked successful!");
    }

    [HttpPost("Unlink")]
    public async Task<ApiResult<string?>> UnlinkAsync([BindUserId] NexusModsUserId userId)
    {
        var tokens = HttpContext.GetSteamTokens();

        if (tokens?.Data is null)
            return ApiBadRequest("Unlinked successful!");

        if (!await _steamStorage.RemoveAsync(userId, tokens.ExternalId))
            return ApiBadRequest("Failed to unlink!");

        return ApiResult("Unlinked successful!");
    }

    [HttpPost("GetUserInfo")]
    public async Task<ApiResult<SteamUserInfo?>> GetUserInfoByAccessTokenAsync(CancellationToken ct)
    {
        var tokens = HttpContext.GetSteamTokens();

        if (tokens?.Data is null)
            return ApiBadRequest("Failed to get the token!");

        var result = await _steamAPIClient.GetUserInfoAsync(tokens.ExternalId, ct);
        return ApiResult(result);
    }
}