using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Options;
using BUTR.Site.NexusMods.Server.Repositories;
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
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), ButrNexusModsAuthorization, TenantNotRequired]
public sealed class SteamController : ApiControllerBase
{
    public sealed record SteamOpenIdUrlModel(string Url);


    private readonly ISteamStorage _steamStorage;
    private readonly SteamAPIOptions _options;
    private readonly ISteamCommunityClient _steamCommunityClient;
    private readonly ISteamAPIClient _steamAPIClient;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public SteamController(ISteamStorage steamStorage, IOptions<SteamAPIOptions> options, ISteamCommunityClient steamCommunityClient, ISteamAPIClient steamAPIClient, IUnitOfWorkFactory unitOfWorkFactory)
    {
        _steamStorage = steamStorage;
        _options = options.Value;
        _steamCommunityClient = steamCommunityClient;
        _steamAPIClient = steamAPIClient;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    [HttpPost]
    public async Task<ApiResult<string?>> AddLinkAsync([FromQuery, Required] Dictionary<string, string> queries, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();

        var isValid = await _steamCommunityClient.ConfirmIdentityAsync(queries, ct);
        if (!isValid)
            return ApiBadRequest("Failed to link!");

        if (!SteamUtils.TryParse(queries["openid.claimed_id"], out var steamId))
            return ApiBadRequest("Failed to link!");
        var steamUserId = SteamUserId.From(steamId);

        var userInfo = await _steamAPIClient.GetUserInfoAsync(steamUserId, ct);

        if (!await _steamStorage.CheckOwnedGamesAsync(userId, steamUserId))
            return ApiBadRequest("Failed to link!");

        if (userInfo is null || !await _steamStorage.UpsertAsync(userId, userInfo.Id, queries))
            return ApiBadRequest("Failed to link!");

        return ApiResult("Linked successful!");
    }

    [HttpDelete]
    public async Task<ApiResult<string?>> RemoveLinkAsync()
    {
        var userId = HttpContext.GetUserId();

        var tokens = HttpContext.GetSteamTokens();

        if (tokens?.Data is null)
            return ApiBadRequest("Unlinked successful!");

        if (!await _steamStorage.RemoveAsync(userId, SteamUserId.From(tokens.ExternalId)))
            return ApiBadRequest("Failed to unlink!");

        return ApiResult("Unlinked successful!");
    }

    [HttpGet("OpenIdUrl")]
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

    [HttpGet("UserInfo")]
    public async Task<ApiResult<SteamUserInfo?>> GetUserInfoAsync(CancellationToken ct)
    {
        var tokens = HttpContext.GetSteamTokens();

        if (tokens?.Data is null)
            return ApiBadRequest("Failed to get the token!");

        var result = await _steamAPIClient.GetUserInfoAsync(SteamUserId.From(tokens.ExternalId), ct);
        return ApiResult(result);
    }


    [HttpPost("ModuleManualLinks")]
    [ButrNexusModsAuthorization(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    public async Task<ApiResult<string?>> AddModuleManualLinkAsync([FromQuery, Required] SteamWorkshopModId modId, [FromQuery, Required] ModuleId moduleId, [BindTenant] TenantId tenant)
    {
        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite();

        var steamWorkshopModToModule = new SteamWorkshopModToModuleEntity
        {
            TenantId = tenant,
            SteamWorkshopModId = modId,
            SteamWorkshopMod = unitOfWrite.UpsertEntityFactory.GetOrCreateSteamWorkshopMod(modId),
            ModuleId = moduleId,
            Module = unitOfWrite.UpsertEntityFactory.GetOrCreateModule(moduleId),
            LinkType = ModToModuleLinkType.ByStaff,
            LastUpdateDate = DateTimeOffset.UtcNow,
        };
        unitOfWrite.SteamWorkshopModModules.Upsert(steamWorkshopModToModule);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return ApiResult("Linked successful!");
    }

    [HttpDelete("ModuleManualLinks")]
    [ButrNexusModsAuthorization(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    public async Task<ApiResult<string?>> RemoveModuleManualLinkAsync([FromQuery, Required] SteamWorkshopModId modId, [FromQuery, Required] ModuleId moduleId)
    {
        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite();

        unitOfWrite.SteamWorkshopModModules
            .Remove(x => x.ModuleId == moduleId && x.SteamWorkshopModId == modId && x.LinkType == ModToModuleLinkType.ByStaff);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return ApiResult("Unlinked successful!");
    }

    [HttpPost("ModuleManualLinks/Paginated")]
    [ButrNexusModsAuthorization(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    public async Task<ApiResult<PagingData<LinkedByStaffModuleSteamWorkshopModsModel>?>> GetModuleManualLinkPaginatedAsync([FromBody, Required] PaginatedQuery query, CancellationToken ct)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var paginated = await unitOfRead.SteamWorkshopModModules.GetByStaffPaginatedAsync(query, ct);

        return ApiPagingResult(paginated);
    }
}