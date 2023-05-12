using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

public sealed record GOGUserInfo(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("username")] string Username);

[ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
public sealed class GOGController : ControllerExtended
{
   
    private readonly IGOGStorage _gogStorage;
    private readonly AppDbContext _dbContext;
    private readonly GOGAuthClient _gogAuthClient;
    private readonly GOGEmbedClient _gogEmbedClient;

    public GOGController(IGOGStorage gogStorage, AppDbContext dbContext, GOGAuthClient gogAuthClient, GOGEmbedClient gogEmbedClient)
    {
        _gogStorage = gogStorage ?? throw new ArgumentNullException(nameof(gogStorage));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
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
    public async Task<ActionResult<APIResponse<string?>>> Link([FromQuery] string code)
    {
        var tokens = await _gogAuthClient.GetToken(code, CancellationToken.None);
        if (tokens is null)
            return APIResponseError<string>("Failed to link!");
        
        var userId = HttpContext.GetUserId();

        var games = await _gogEmbedClient.GetGames(tokens.AccessToken, CancellationToken.None);
        if (games is null)
            return APIResponseError<string>("Failed to link!");
        
        var ownsBannerlord = games.Owned.Contains(1802539526) || games.Owned.Contains(1564781494);
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
            if (!await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsUserMetadataEntity>(x => x.NexusModsUserId == userId, ApplyChanges))
                return APIResponseError<string>("Failed to link!");
        }

        if (!_gogStorage.Upsert(userId, tokens.UserId, tokens))
            return APIResponseError<string>("Failed to link!");

        return APIResponse("Linked successful!");
    }

    [HttpPost("Unlink")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> Unlink()
    {
        var userId = HttpContext.GetUserId();
        var tokens = HttpContext.GetGOGTokens();

        if (tokens?.Data is null)
            return APIResponseError<string>("Unlinked successful!");

        if (!_gogStorage.Remove(userId, tokens.ExternalId))
            return APIResponseError<string>("Failed to unlink!");

        return APIResponse("Unlinked successful!");
    }

    [HttpPost("GetUserInfo")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<GOGUserInfo?>>> GetUserInfoByAccessToken()
    {
        var tokens = HttpContext.GetGOGTokens();

        if (tokens?.Data is null)
            return APIResponseError<GOGUserInfo>("Failed to get the token!");

        var result = await _gogEmbedClient.GetUserInfo(tokens.Data.AccessToken, CancellationToken.None);
        return APIResponse(result);
    }
}