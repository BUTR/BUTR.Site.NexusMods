using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Options;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

public sealed record SteamUserInfo(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("username")] string Username);

[ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
public sealed class SteamController : ControllerExtended
{
    private readonly ISteamStorage _steamStorage;
    private readonly AppDbContext _dbContext;
    private readonly SteamAPIOptions _options;
    private readonly SteamCommunityClient _steamCommunityClient;
    private readonly SteamAPIClient _steamAPIClient;

    public SteamController(ISteamStorage steamStorage, AppDbContext dbContext, IOptions<SteamAPIOptions> options, SteamCommunityClient steamCommunityClient, SteamAPIClient steamAPIClient)
    {
        _steamStorage = steamStorage ?? throw new ArgumentNullException(nameof(steamStorage));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _steamCommunityClient = steamCommunityClient ?? throw new ArgumentNullException(nameof(steamCommunityClient));
        _steamAPIClient = steamAPIClient ?? throw new ArgumentNullException(nameof(steamAPIClient));
    }

    [HttpGet("GetOpenIdUrl")]
    [Produces("application/json")]
    public ActionResult<APIResponse<SteamOpenIdUrlModel?>> GetOpenIdUrl()
    {
        var query = QueryString.Create(new KeyValuePair<string, string?>[]
        {
            new("openid.ns", "http://specs.openid.net/auth/2.0"),
            new("openid.mode", "checkid_setup"),
            new("openid.return_to", _options.RedirectUri),
            new("openid.realm", _options.Realm),
            new("openid.identity", "http://specs.openid.net/auth/2.0/identifier_select"),
            new("openid.claimed_id", "http://specs.openid.net/auth/2.0/identifier_select"),
        });
        var steamLoginUrl = new UriBuilder("https://steamcommunity.com/openid/login") { Query = query.ToUriComponent() };

        return APIResponse(new SteamOpenIdUrlModel(steamLoginUrl.ToString()));
    }

    [HttpGet("Link")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> Link([FromQuery] Dictionary<string, string> _, CancellationToken ct)
    {
        var queries = HttpContext.Request.Query.ToDictionary(x => x.Key, x => x.Value[0] ?? string.Empty);

        var isValid = await _steamCommunityClient.ConfirmIdentity(queries, ct);
        if (!isValid)
            return APIResponseError<string>("Failed to link!");

        if (!SteamUtils.TryParse(queries["openid.claimed_id"], out var steamId))
            return APIResponseError<string>("Failed to link!");

        var userId = HttpContext.GetUserId();
        var userInfo = await _steamAPIClient.GetUserInfo(steamId, ct);

        var ownsBannerlord = await _steamAPIClient.IsOwningGame(steamId, 261550, ct);
        if (ownsBannerlord)
        {
            NexusModsUserMetadataEntity? ApplyChanges(NexusModsUserMetadataEntity? existing) => existing switch
            {
                null => null,
                _ => existing with
                {
                    Metadata = existing.Metadata.SetAndReturn("MB2B", "")
                },
            };
            if (!await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsUserMetadataEntity>(x => x.NexusModsUserId == userId, ApplyChanges, CancellationToken.None))
                return APIResponseError<string>("Failed to link!");
        }

        if (userInfo is null || !await _steamStorage.UpsertAsync(userId, userInfo.Id, queries))
            return APIResponseError<string>("Failed to link!");

        return APIResponse("Linked successful!");
    }

    [HttpPost("Unlink")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> Unlink()
    {
        var userId = HttpContext.GetUserId();
        var tokens = HttpContext.GetSteamTokens();

        if (tokens?.Data is null)
            return APIResponseError<string>("Unlinked successful!");

        if (!await _steamStorage.RemoveAsync(userId, tokens.ExternalId))
            return APIResponseError<string>("Failed to unlink!");

        return APIResponse("Unlinked successful!");
    }

    [HttpPost("GetUserInfo")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<SteamUserInfo?>>> GetUserInfoByAccessToken(CancellationToken ct)
    {
        var tokens = HttpContext.GetSteamTokens();

        if (tokens?.Data is null)
            return APIResponseError<SteamUserInfo>("Failed to get the token!");

        var result = await _steamAPIClient.GetUserInfo(tokens.ExternalId, ct);
        return APIResponse(result);
    }
}