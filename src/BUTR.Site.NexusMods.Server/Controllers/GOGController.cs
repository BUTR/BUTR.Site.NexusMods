using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
public sealed class GOGController : ControllerExtended
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
    [Produces("application/json")]
    public ActionResult<APIResponse<GOGOAuthUrlModel?>> GetOpenIdUrl()
    {
        return APIResponse(new GOGOAuthUrlModel(_gogAuthClient.GetOAuth2Url()));
    }

    [HttpGet("Link")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> LinkAsync([FromQuery] string code, CancellationToken ct)
    {
        var tokens = await _gogAuthClient.CreateTokensAsync(code, ct);
        if (tokens is null)
            return APIResponseError<string>("Failed to link!");

        var userId = HttpContext.GetUserId();

        if (!await _gogStorage.CheckOwnedGamesAsync(userId, tokens.UserId, tokens))
            return APIResponseError<string>("Failed to link!");

        if (!await _gogStorage.UpsertAsync(userId, tokens.UserId, tokens))
            return APIResponseError<string>("Failed to link!");

        return APIResponse("Linked successful!");
    }

    [HttpPost("Unlink")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> UnlinkAsync()
    {
        var userId = HttpContext.GetUserId();
        var tokens = HttpContext.GetGOGTokens();

        if (tokens?.Data is null)
            return APIResponseError<string>("Unlinked successful!");

        if (!await _gogStorage.RemoveAsync(userId, tokens.ExternalId))
            return APIResponseError<string>("Failed to unlink!");

        return APIResponse("Unlinked successful!");
    }

    [HttpPost("GetUserInfo")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<GOGUserInfo?>>> GetUserInfoByAccessTokenAsync(CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        var tokens = HttpContext.GetGOGTokens();

        if (tokens?.Data is null)
            return APIResponseError<GOGUserInfo>("Failed to get the token!");

        var refreshed = await _gogAuthClient.GetOrRefreshTokensAsync(tokens.Data, ct);
        if (refreshed is null)
            return APIResponse<GOGUserInfo>(null);

        if (tokens.Data.AccessToken != refreshed.AccessToken)
            await _gogStorage.UpsertAsync(userId, refreshed.UserId, refreshed);

        var result = await _gogEmbedClient.GetUserInfoAsync(refreshed.AccessToken, ct);
        return APIResponse(result);
    }
}