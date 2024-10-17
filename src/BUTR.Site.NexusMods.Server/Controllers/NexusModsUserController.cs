using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Repositories;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.BindingSources;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), ButrNexusModsAuthorization, TenantRequired]
public sealed class NexusModsUserController : ApiControllerBase
{
    private readonly ILogger _logger;
    private readonly INexusModsAPIClient _nexusModsAPIClient;
    private readonly INexusModsAPIv2Client _nexusModsAPIv2Client;
    private readonly INexusModsModFileParser _nexusModsModFileParser;
    private readonly ISteamAPIClient _steamAPIClient;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public NexusModsUserController(ILogger<NexusModsUserController> logger, INexusModsAPIClient nexusModsAPIClient, INexusModsAPIv2Client nexusModsAPIv2Client, INexusModsModFileParser nexusModsModFileParser, ISteamAPIClient steamAPIClient, IUnitOfWorkFactory unitOfWorkFactory)
    {
        _logger = logger;
        _nexusModsAPIClient = nexusModsAPIClient;
        _nexusModsAPIv2Client = nexusModsAPIv2Client;
        _nexusModsModFileParser = nexusModsModFileParser;
        _steamAPIClient = steamAPIClient;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    private async Task<NexusModsUserId> GetUserIdAsync(NexusModsUserId? userId, NexusModsUserName? username, CancellationToken ct) => userId switch
    {
        null when username is not null => await _nexusModsAPIv2Client.GetUserIdAsync(HttpContext.GetAPIKey(), username.Value, ct),
        null when username is null => HttpContext.GetUserId(),
        _ => NexusModsUserId.None
    };

    [HttpGet("Profile")]
    public ApiResult<ProfileModel?> GetProfile() => ApiResult(HttpContext.GetProfile());


    [HttpPatch("Role")]
    [ButrNexusModsAuthorization(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    public async Task<ApiResult<string?>> SetRoleAsync([FromQuery, Required] ApplicationRole role, [FromQuery] NexusModsUserId? userId, [FromQuery] NexusModsUserName? username, [BindTenant] TenantId tenant, CancellationToken ct)
    {
        var nexusModsUserId = await GetUserIdAsync(userId, username, ct);
        if (nexusModsUserId == NexusModsUserId.None)
            return ApiBadRequest("User not found!");

        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite();

        unitOfWrite.NexusModsUsers.Upsert(NexusModsUserEntity.CreateWithRole(nexusModsUserId, tenant, role));

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return ApiResult("Set successful!");
    }

    [HttpDelete("Role")]
    [ButrNexusModsAuthorization(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    public async Task<ApiResult<string?>> RemoveRoleAsync([FromQuery] NexusModsUserId? userId, [FromQuery] NexusModsUserName? username, [BindTenant] TenantId tenant, CancellationToken ct)
    {
        var nexusModsUserId = await GetUserIdAsync(userId, username, ct);
        if (nexusModsUserId == NexusModsUserId.None)
            return ApiBadRequest("User not found!");

        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite();

        unitOfWrite.NexusModsUsers.Upsert(NexusModsUserEntity.CreateWithRole(nexusModsUserId, tenant, ApplicationRole.User));

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return ApiResult("Removed successful!");
    }


    [HttpPost("NexusModsMods/Paginated")]
    public async Task<ApiResult<PagingData<UserLinkedNexusModsModModel>?>> GetNexusModsModsPaginatedAsync([FromBody, Required] PaginatedQuery query, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();

        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var paginated = await unitOfRead.NexusModsUsers.GetNexusModsModsPaginatedAsync(userId, query, ct);

        return ApiPagingResult(paginated);
    }

    [HttpPost("NexusModsMods/Available/Paginated")]
    public async Task<ApiResult<PagingData<UserAvailableNexusModsModModel>?>> GetNexusModsModsPaginateAvailabledAsync([FromBody, Required] PaginatedQuery query, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();

        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var paginated = await unitOfRead.NexusModsUsers.GetAvailableNexusModsModsPaginatedAsync(userId, query, ct);

        return ApiPagingResult(paginated);
    }


    [HttpPost("NexusModsModLinks")]
    public async Task<ApiResult<string?>> AddNexusModsModLinkAsync([FromQuery, Required] NexusModsModId modId, [FromQuery] NexusModsUserId? userId, [FromQuery] NexusModsUserName? username, [BindTenant] TenantId tenant, CancellationToken ct)
    {
        var nexusModsUserId = await GetUserIdAsync(userId, username, ct);
        if (nexusModsUserId == NexusModsUserId.None)
            return ApiBadRequest("User not found!");

        var currentUserId = HttpContext.GetUserId();
        if (currentUserId != nexusModsUserId && HttpContext.GetRole() != ApplicationRoles.Moderator && HttpContext.GetRole() != ApplicationRoles.Administrator)
            return ApiBadRequest("Permission denied!");

        if (HttpContext.GetAPIKey() is { } apiKey && apiKey != NexusModsApiKey.None)
            return await AddNexusModsModLinkWithApiKeyAsync(apiKey, modId, nexusModsUserId, tenant, ct);

        // TODO:
        return ApiBadRequest("Token auth not supported yet!");
    }
    private async Task<ApiResult<string?>> AddNexusModsModLinkWithApiKeyAsync(NexusModsApiKey apiKey, NexusModsModId modId, NexusModsUserId userId, TenantId tenant, CancellationToken ct)
    {
        var gameDomain = tenant.ToGameDomain();

        if (await _nexusModsAPIClient.GetModAsync(gameDomain, modId, apiKey, ct) is not { } modInfo)
            return ApiBadRequest("Mod not found!");

        if (userId != modInfo.User.Id)
            return ApiBadRequest("User does not have access to the mod!");

        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite();

        /* Offloading this to a background task
        if (HttpContext.GetIsPremium()) // Premium is needed for API based downloading
        {
            var response = await _nexusModsAPIClient.GetModFileInfosFullAsync(gameDomain, modInfo.Id, apiKey, ct);
            if (response is null) return ApiBadRequest("Error while fetching the mod!");

            var entities = await _nexusModsModFileParser.GetModuleInfosAsync(gameDomain, modInfo.Id, response.Files, apiKey, ct)
                .Select(x => x.ModuleInfo)
                .GroupBy(x => x.Id)
                .SelectAwaitWithCancellation(async (x, ct2) => await x.FirstOrDefaultAsync(ct2))
                .Select(y => new NexusModsModToModuleEntity
                {
                    TenantId = tenant,
                    NexusModsModId = modId,
                    NexusModsMod = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsMod(modId),
                    ModuleId = ModuleId.From(y.Id),
                    Module = unitOfWrite.UpsertEntityFactory.GetOrCreateModule(ModuleId.From(y.Id)),
                    LinkType = NexusModsModToModuleLinkType.ByUnverifiedFileExposure,
                    LastUpdateDate = DateTimeOffset.UtcNow
                }).ToArrayAsync(ct);
            unitOfWrite.NexusModsModModules.UpsertRange(entities);
        }
        */

        var nexusModsModToName = new NexusModsModToNameEntity
        {
            TenantId = tenant,
            NexusModsModId = modId,
            NexusModsMod = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsMod(modId),
            Name = modInfo.Name,
        };
        unitOfWrite.NexusModsModName.Upsert(nexusModsModToName);

        var nexusModsUserToNexusModsMod = new NexusModsUserToNexusModsModEntity
        {
            TenantId = tenant,
            NexusModsUserId = userId,
            NexusModsUser = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsUser(userId),
            NexusModsModId = modId,
            NexusModsMod = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsMod(modId),
            LinkType = NexusModsUserToModLinkType.ByAPIConfirmation,
        };
        unitOfWrite.NexusModsUserToNexusModsMods.Upsert(nexusModsUserToNexusModsMod);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return ApiResult("Linked successful!");
    }

    [HttpPatch("NexusModsModLinks")]
    public async Task<ApiResult<string?>> UpdateNexusModsModLinkAsync([FromQuery, Required] NexusModsModId modId, [FromQuery] NexusModsUserId? userId, [FromQuery] NexusModsUserName? username, [BindTenant] TenantId tenant, CancellationToken ct)
    {
        var nexusModsUserId = await GetUserIdAsync(userId, username, ct);
        if (nexusModsUserId == NexusModsUserId.None)
            return ApiBadRequest("User not found!");

        var currentUserId = HttpContext.GetUserId();
        if (currentUserId != nexusModsUserId && HttpContext.GetRole() != ApplicationRoles.Moderator && HttpContext.GetRole() != ApplicationRoles.Administrator)
            return ApiBadRequest("Permission denied!");

        if (HttpContext.GetAPIKey() is { } apiKey && apiKey != NexusModsApiKey.None)
            return await UpdateNexusModsModLinkWithApiKeyAsync(apiKey, modId, nexusModsUserId, tenant, ct);

        // TODO:
        return ApiBadRequest("Token auth not supported yet!");
    }
    private async Task<ApiResult<string?>> UpdateNexusModsModLinkWithApiKeyAsync(NexusModsApiKey apiKey, NexusModsModId modId, NexusModsUserId userId, TenantId tenant, CancellationToken ct)
    {
        var gameDomain = tenant.ToGameDomain();

        if (await _nexusModsAPIClient.GetModAsync(gameDomain, modId, apiKey, ct) is not { } modInfo)
            return ApiBadRequest("Mod not found!");

        if (userId != modInfo.User.Id)
            return ApiBadRequest("User does not have access to the mod!");

        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite();

        var nexusModsModToName = new NexusModsModToNameEntity
        {
            TenantId = tenant,
            NexusModsModId = modId,
            NexusModsMod = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsMod(modId),
            Name = modInfo.Name,
        };
        unitOfWrite.NexusModsModName.Upsert(nexusModsModToName);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return ApiResult("Updated successful!");
    }

    [HttpDelete("NexusModsModLinks")]
    public async Task<ApiResult<string?>> RemoveNexusModsModLinkAsync([FromQuery, Required] NexusModsModId modId, [FromQuery] NexusModsUserId? userId, [FromQuery] NexusModsUserName? username, CancellationToken ct)
    {
        var nexusModsUserId = await GetUserIdAsync(userId, username, ct);
        if (nexusModsUserId == NexusModsUserId.None)
            return ApiBadRequest("User not found!");

        var currentUserId = HttpContext.GetUserId();
        if (currentUserId != nexusModsUserId && HttpContext.GetRole() != ApplicationRoles.Moderator && HttpContext.GetRole() != ApplicationRoles.Administrator)
            return ApiBadRequest("Permission denied!");

        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite();

        unitOfWrite.NexusModsUserToNexusModsMods
            .Remove(x => x.NexusModsUserId == nexusModsUserId && x.NexusModsModId == modId && x.LinkType == NexusModsUserToModLinkType.ByOwner);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return ApiResult("Unlinked successful!");
    }


    [HttpPost("SteamWorkshopMods/Paginated")]
    public async Task<ApiResult<PagingData<UserLinkedSteamWorkshopModModel>?>> GetSteamWorkshopModsPaginatedAsync([FromBody, Required] PaginatedQuery query, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();

        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var paginated = await unitOfRead.NexusModsUsers.GetSteamWorkshopModsPaginatedAsync(userId, query, ct);

        return ApiPagingResult(paginated);
    }

    [HttpPost("SteamWorkshopMods/Available/Paginated")]
    public async Task<ApiResult<PagingData<UserAvailableSteamWorkshopModModel>?>> GetSteamWorkshopModsPaginateAvailabledAsync([FromBody, Required] PaginatedQuery query, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();

        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var paginated = await unitOfRead.NexusModsUsers.GetAvailableSteamWorkshopModsPaginatedAsync(userId, query, ct);

        return ApiPagingResult(paginated);
    }


    [HttpPost("SteamWorkshopModsLinks/ImportAll")]
    public async Task<ApiResult<string?>> AddSteamWorkshopModLinkImportAllAsync([FromQuery] NexusModsUserId? userId, [FromQuery] NexusModsUserName? username, [BindTenant] TenantId tenant, CancellationToken ct)
    {
        var nexusModsUserId = await GetUserIdAsync(userId, username, ct);
        if (nexusModsUserId == NexusModsUserId.None)
            return ApiBadRequest("User not found!");

        var currentUserId = HttpContext.GetUserId();
        if (currentUserId != nexusModsUserId && HttpContext.GetRole() != ApplicationRoles.Moderator && HttpContext.GetRole() != ApplicationRoles.Administrator)
            return ApiBadRequest("Permission denied!");

        var tokens = HttpContext.GetSteamTokens();
        if (tokens is null)
            return ApiBadRequest("Steam not linked!");

        var steamUserId = SteamUserId.From(tokens.ExternalId);

        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite();
        foreach (var steamAppId in TenantUtils.FromTenantToSteamAppIds(tenant.Value))
        {
            foreach (var steamWorkshopItemInfo in await _steamAPIClient.GetAllOwnedWorkshopItemAsync(steamUserId, steamAppId, ct))
            {
                var steamWorkshopModToName = new SteamWorkshopModToNameEntity
                {
                    TenantId = tenant,
                    SteamWorkshopModId = steamWorkshopItemInfo.ModId,
                    SteamWorkshopMod = unitOfWrite.UpsertEntityFactory.GetOrCreateSteamWorkshopMod(steamWorkshopItemInfo.ModId),
                    Name = steamWorkshopItemInfo.Name,
                };
                unitOfWrite.SteamWorkshopModName.Upsert(steamWorkshopModToName);

                var nexusModsUserToSteamWorkshopMod = new NexusModsUserToSteamWorkshopModEntity
                {
                    TenantId = tenant,
                    NexusModsUserId = nexusModsUserId,
                    NexusModsUser = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsUser(nexusModsUserId),
                    SteamWorkshopModId = steamWorkshopItemInfo.ModId,
                    SteamWorkshopMod = unitOfWrite.UpsertEntityFactory.GetOrCreateSteamWorkshopMod(steamWorkshopItemInfo.ModId),
                    LinkType = NexusModsUserToModLinkType.ByAPIConfirmation,
                };
                unitOfWrite.NexusModsUserToSteamWorkshopMods.Upsert(nexusModsUserToSteamWorkshopMod);

            }
        }

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return ApiResult("Linked successful!");
    }

    [HttpPost("SteamWorkshopModsLinks")]
    public async Task<ApiResult<string?>> AddSteamWorkshopModLinkAsync([FromQuery, Required] SteamWorkshopModId modId, [FromQuery] NexusModsUserId? userId, [FromQuery] NexusModsUserName? username, [BindTenant] TenantId tenant, CancellationToken ct)
    {
        var nexusModsUserId = await GetUserIdAsync(userId, username, ct);
        if (nexusModsUserId == NexusModsUserId.None)
            return ApiBadRequest("User not found!");

        var currentUserId = HttpContext.GetUserId();
        if (currentUserId != nexusModsUserId && HttpContext.GetRole() != ApplicationRoles.Moderator && HttpContext.GetRole() != ApplicationRoles.Administrator)
            return ApiBadRequest("Permission denied!");

        var tokens = HttpContext.GetSteamTokens();
        if (tokens is null)
            return ApiBadRequest("Steam not linked!");

        var steamUserId = SteamUserId.From(tokens.ExternalId);

        return await AddSteamWorkshopModsLinkWithApiKeyAsync(steamUserId, modId, nexusModsUserId, tenant, ct);
    }
    private async Task<ApiResult<string?>> AddSteamWorkshopModsLinkWithApiKeyAsync(SteamUserId steamUserId, SteamWorkshopModId modId, NexusModsUserId userId, TenantId tenant, CancellationToken ct)
    {
        var steamWorkshopItemInfo = default(SteamWorkshopItemInfo);
        foreach (var steamAppId in TenantUtils.FromTenantToSteamAppIds(tenant.Value))
        {
            steamWorkshopItemInfo = await _steamAPIClient.GetOwnedWorkshopItemAsync(steamUserId, steamAppId, modId, ct);
            if (steamWorkshopItemInfo is not null) break;
        }

        if (steamWorkshopItemInfo is null)
            return ApiBadRequest("Steam Workshop Item not owned!");

        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite();

        var steamWorkshopModToName = new SteamWorkshopModToNameEntity
        {
            TenantId = tenant,
            SteamWorkshopModId = modId,
            SteamWorkshopMod = unitOfWrite.UpsertEntityFactory.GetOrCreateSteamWorkshopMod(modId),
            Name = steamWorkshopItemInfo.Name,
        };
        unitOfWrite.SteamWorkshopModName.Upsert(steamWorkshopModToName);

        var nexusModsUserToSteamWorkshopMod = new NexusModsUserToSteamWorkshopModEntity
        {
            TenantId = tenant,
            NexusModsUserId = userId,
            NexusModsUser = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsUser(userId),
            SteamWorkshopModId = modId,
            SteamWorkshopMod = unitOfWrite.UpsertEntityFactory.GetOrCreateSteamWorkshopMod(modId),
            LinkType = NexusModsUserToModLinkType.ByAPIConfirmation,
        };
        unitOfWrite.NexusModsUserToSteamWorkshopMods.Upsert(nexusModsUserToSteamWorkshopMod);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return ApiResult("Linked successful!");
    }

    [HttpPatch("SteamWorkshopModsLinks")]
    public async Task<ApiResult<string?>> UpdateSteamWorkshopModLinkAsync([FromQuery, Required] SteamWorkshopModId modId, [FromQuery] NexusModsUserId? userId, [FromQuery] NexusModsUserName? username, [BindTenant] TenantId tenant, CancellationToken ct)
    {
        var nexusModsUserId = await GetUserIdAsync(userId, username, ct);
        if (nexusModsUserId == NexusModsUserId.None)
            return ApiBadRequest("User not found!");

        var currentUserId = HttpContext.GetUserId();
        if (currentUserId != nexusModsUserId && HttpContext.GetRole() != ApplicationRoles.Moderator && HttpContext.GetRole() != ApplicationRoles.Administrator)
            return ApiBadRequest("Permission denied!");

        var tokens = HttpContext.GetSteamTokens();
        if (tokens is null)
            return ApiBadRequest("Steam not linked!");

        var steamUserId = SteamUserId.From(tokens.ExternalId);

        return await UpdateSteamWorkshopModLinkWithApiKeyAsync(steamUserId, modId, nexusModsUserId, tenant, ct);
    }
    private async Task<ApiResult<string?>> UpdateSteamWorkshopModLinkWithApiKeyAsync(SteamUserId steamUserId, SteamWorkshopModId modId, NexusModsUserId userId, TenantId tenant, CancellationToken ct)
    {
        var steamWorkshopItemInfo = default(SteamWorkshopItemInfo);
        foreach (var steamAppId in TenantUtils.FromTenantToSteamAppIds(tenant.Value))
        {
            steamWorkshopItemInfo = await _steamAPIClient.GetOwnedWorkshopItemAsync(steamUserId, steamAppId, modId, ct);
            if (steamWorkshopItemInfo is not null) break;
        }

        if (steamWorkshopItemInfo is null)
            return ApiBadRequest("Steam Workshop Item not owned!");

        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite();

        var steamWorkshopModToName = new SteamWorkshopModToNameEntity
        {
            TenantId = tenant,
            SteamWorkshopModId = modId,
            SteamWorkshopMod = unitOfWrite.UpsertEntityFactory.GetOrCreateSteamWorkshopMod(modId),
            Name = steamWorkshopItemInfo.Name,
        };
        unitOfWrite.SteamWorkshopModName.Upsert(steamWorkshopModToName);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return ApiResult("Updated successful!");
    }

    [HttpDelete("SteamWorkshopModsLinks")]
    public async Task<ApiResult<string?>> RemoveSteamWorkshopModLinkAsync([FromQuery, Required] SteamWorkshopModId modId, [FromQuery] NexusModsUserId? userId, [FromQuery] NexusModsUserName? username, CancellationToken ct)
    {
        var nexusModsUserId = await GetUserIdAsync(userId, username, ct);
        if (nexusModsUserId == NexusModsUserId.None)
            return ApiBadRequest("User not found!");

        var currentUserId = HttpContext.GetUserId();
        if (currentUserId != nexusModsUserId && HttpContext.GetRole() != ApplicationRoles.Moderator && HttpContext.GetRole() != ApplicationRoles.Administrator)
            return ApiBadRequest("Permission denied!");

        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite();

        unitOfWrite.NexusModsUserToSteamWorkshopMods
            .Remove(x => x.NexusModsUserId == nexusModsUserId && x.SteamWorkshopModId == modId && x.LinkType == NexusModsUserToModLinkType.ByOwner);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return ApiResult("Unlinked successful!");
    }


    [HttpPost("ModuleManualLinks")]
    [ButrNexusModsAuthorization(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    public async Task<ApiResult<string?>> AddModuleManualLinkAsync([FromQuery, Required] ModuleId moduleId, [FromQuery] NexusModsUserId? userId, [FromQuery] NexusModsUserName? username, [BindTenant] TenantId tenant, CancellationToken ct)
    {
        var nexusModsUserId = await GetUserIdAsync(userId, username, ct);
        if (nexusModsUserId == NexusModsUserId.None)
            return ApiBadRequest("User not found!");

        var currentUserId = HttpContext.GetUserId();
        if (currentUserId != nexusModsUserId && HttpContext.GetRole() != ApplicationRoles.Moderator && HttpContext.GetRole() != ApplicationRoles.Administrator)
            return ApiBadRequest("Permission denied!");

        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite();

        var nexusModsUserToModule = new NexusModsUserToModuleEntity
        {
            TenantId = tenant,
            NexusModsUserId = nexusModsUserId,
            NexusModsUser = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsUser(nexusModsUserId),
            ModuleId = moduleId,
            Module = unitOfWrite.UpsertEntityFactory.GetOrCreateModule(moduleId),
            LinkType = NexusModsUserToModuleLinkType.ByStaff,
        };
        unitOfWrite.NexusModsUserToModules.Upsert(nexusModsUserToModule);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return ApiResult("Allowed successful!");
    }

    [HttpDelete("ModuleManualLinks")]
    [ButrNexusModsAuthorization(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    public async Task<ApiResult<string?>> RemoveModuleManualLinkAsync([FromQuery, Required] ModuleId moduleId, [FromQuery] NexusModsUserId? userId, [FromQuery] NexusModsUserName? username, CancellationToken ct)
    {
        var nexusModsUserId = await GetUserIdAsync(userId, username, ct);
        if (nexusModsUserId == NexusModsUserId.None)
            return ApiBadRequest("User not found!");

        var currentUserId = HttpContext.GetUserId();
        if (currentUserId != nexusModsUserId && HttpContext.GetRole() != ApplicationRoles.Moderator && HttpContext.GetRole() != ApplicationRoles.Administrator)
            return ApiBadRequest("Permission denied!");

        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite();

        unitOfWrite.NexusModsUserToModules
            .Remove(x => x.NexusModsUserId == nexusModsUserId && x.ModuleId == moduleId && x.LinkType == NexusModsUserToModuleLinkType.ByStaff);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return ApiResult("Disallowed successful!");
    }

    [HttpPost("ModuleManualLinks/Paginated")]
    [ButrNexusModsAuthorization(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    public async Task<ApiResult<PagingData<UserManuallyLinkedModuleModel>?>> GetModuleManualLinkPaginatedAsync([FromBody, Required] PaginatedQuery query, CancellationToken ct)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var paginated = await unitOfRead.NexusModsUserToModules.GetManuallyLinkedModuleIdsPaginatedAsync(query, NexusModsUserToModuleLinkType.ByStaff, ct);

        return ApiPagingResult(paginated);
    }


    [HttpPost("NexusModsModManualLinks")]
    public async Task<ApiResult<string?>> AddNexusModsModManualLinkAsync([FromQuery, Required] NexusModsModId modId, [FromQuery] NexusModsUserId? userId, [FromQuery] NexusModsUserName? username, [BindTenant] TenantId tenant, CancellationToken ct)
    {
        var nexusModsUserId = await GetUserIdAsync(userId, username, ct);
        if (nexusModsUserId == NexusModsUserId.None)
            return ApiBadRequest("User not found!");

        var currentUserId = HttpContext.GetUserId();
        if (currentUserId != nexusModsUserId && HttpContext.GetRole() != ApplicationRoles.Moderator && HttpContext.GetRole() != ApplicationRoles.Administrator)
            return ApiBadRequest("Permission denied!");

        if (HttpContext.GetAPIKey() is { } apiKey && apiKey != NexusModsApiKey.None)
            return await AddNexusModsModManualLinkWithApiKeyAsync(apiKey, modId, nexusModsUserId, tenant, ct);

        // TODO:
        return ApiBadRequest("Token auth not supported yet!");
    }
    private async Task<ApiResult<string?>> AddNexusModsModManualLinkWithApiKeyAsync(NexusModsApiKey apiKey, NexusModsModId modId, NexusModsUserId userId, TenantId tenant, CancellationToken ct)
    {
        var gameDomain = tenant.ToGameDomain();

        if (await _nexusModsAPIClient.GetModAsync(gameDomain, modId, apiKey, ct) is not { } modInfo)
            return ApiBadRequest("Mod not found!");

        if (userId != modInfo.User.Id)
            return ApiBadRequest("User does not have access to the mod!");

        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite();

        var nexusModsModToName = new NexusModsModToNameEntity
        {
            TenantId = tenant,
            NexusModsModId = modId,
            NexusModsMod = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsMod(modId),
            Name = modInfo.Name,
        };
        unitOfWrite.NexusModsModName.Upsert(nexusModsModToName);

        var nexusModsUserToNexusModsMods = new NexusModsUserToNexusModsModEntity[]
        {
            new()
            {
                TenantId = tenant,
                NexusModsUserId = userId,
                NexusModsUser = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsUser(userId),
                NexusModsModId = modId,
                NexusModsMod = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsMod(modId),
                LinkType = NexusModsUserToModLinkType.ByAPIConfirmation,
            },
            new()
            {
                TenantId = tenant,
                NexusModsUserId = userId,
                NexusModsUser = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsUser(userId),
                NexusModsModId = modId,
                NexusModsMod = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsMod(modId),
                LinkType = NexusModsUserToModLinkType.ByOwner,
            }
        };
        unitOfWrite.NexusModsUserToNexusModsMods.UpsertRange(nexusModsUserToNexusModsMods);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return ApiResult("Allowed successful!");
    }

    [HttpDelete("NexusModsModManualLinks")]
    public async Task<ApiResult<string?>> RemoveNexusModsModManualLinkAsync([FromQuery, Required] NexusModsModId modId, [FromQuery] NexusModsUserId? userId, [FromQuery] NexusModsUserName? username, CancellationToken ct)
    {
        var nexusModsUserId = await GetUserIdAsync(userId, username, ct);
        if (nexusModsUserId == NexusModsUserId.None)
            return ApiBadRequest("User not found!");

        var currentUserId = HttpContext.GetUserId();
        if (currentUserId != nexusModsUserId && HttpContext.GetRole() != ApplicationRoles.Moderator && HttpContext.GetRole() != ApplicationRoles.Administrator)
            return ApiBadRequest("Permission denied!");

        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite();

        unitOfWrite.NexusModsUserToNexusModsMods
            .Remove(x => x.NexusModsUserId == nexusModsUserId && x.NexusModsModId == modId && x.LinkType == NexusModsUserToModLinkType.ByOwner);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return ApiResult("Disallowed successful!");
    }

    [HttpPost("NexusModsModManualLinks/Paginated")]
    public async Task<ApiResult<PagingData<UserManuallyLinkedNexusModsModModel>?>> GetNexusModsModManualLinkPaginatedAsync([FromBody] PaginatedQuery query, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();

        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var paginated = await unitOfRead.NexusModsUserToNexusModsMods.GetManuallyLinkedPaginatedAsync(userId, query, ct);

        return ApiPagingResult(paginated);
    }


    [HttpPost("SteamWorkshopModManualLinks")]
    public async Task<ApiResult<string?>> AddSteamWorkshopModManualLinkAsync([FromQuery, Required] SteamWorkshopModId modId, [FromQuery] NexusModsUserId? userId, [FromQuery] NexusModsUserName? username, [BindTenant] TenantId tenant, CancellationToken ct)
    {
        var nexusModsUserId = await GetUserIdAsync(userId, username, ct);
        if (nexusModsUserId == NexusModsUserId.None)
            return ApiBadRequest("User not found!");

        var currentUserId = HttpContext.GetUserId();
        if (currentUserId != nexusModsUserId && HttpContext.GetRole() != ApplicationRoles.Moderator && HttpContext.GetRole() != ApplicationRoles.Administrator)
            return ApiBadRequest("Permission denied!");

        var tokens = HttpContext.GetSteamTokens();
        if (tokens is null)
            return ApiBadRequest("Steam not linked!");

        var steamUserId = SteamUserId.From(tokens.ExternalId);

        return await AddSteamWorkshopModManualLinkWithApiKeyAsync(steamUserId, modId, nexusModsUserId, tenant, ct);
    }
    private async Task<ApiResult<string?>> AddSteamWorkshopModManualLinkWithApiKeyAsync(SteamUserId steamUserId, SteamWorkshopModId modId, NexusModsUserId userId, TenantId tenant, CancellationToken ct)
    {
        var steamWorkshopItemInfo = default(SteamWorkshopItemInfo);
        foreach (var steamAppId in TenantUtils.FromTenantToSteamAppIds(tenant.Value))
        {
            steamWorkshopItemInfo = await _steamAPIClient.GetOwnedWorkshopItemAsync(steamUserId, steamAppId, modId, ct);
            if (steamWorkshopItemInfo is not null) break;
        }

        if (steamWorkshopItemInfo is null)
            return ApiBadRequest("Steam Workshop Item not owned!");

        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite();

        var steamWorkshopModToName = new SteamWorkshopModToNameEntity
        {
            TenantId = tenant,
            SteamWorkshopModId = modId,
            SteamWorkshopMod = unitOfWrite.UpsertEntityFactory.GetOrCreateSteamWorkshopMod(modId),
            Name = steamWorkshopItemInfo.Name,
        };
        unitOfWrite.SteamWorkshopModName.Upsert(steamWorkshopModToName);

        var nexusModsUserToSteamWorkshopMod = new NexusModsUserToSteamWorkshopModEntity[]
        {
            new()
            {
                TenantId = tenant,
                NexusModsUserId = userId,
                NexusModsUser = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsUser(userId),
                SteamWorkshopModId = modId,
                SteamWorkshopMod = unitOfWrite.UpsertEntityFactory.GetOrCreateSteamWorkshopMod(modId),
                LinkType = NexusModsUserToModLinkType.ByAPIConfirmation,
            },
            new()
            {
                TenantId = tenant,
                NexusModsUserId = userId,
                NexusModsUser = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsUser(userId),
                SteamWorkshopModId = modId,
                SteamWorkshopMod = unitOfWrite.UpsertEntityFactory.GetOrCreateSteamWorkshopMod(modId),
                LinkType = NexusModsUserToModLinkType.ByOwner,
            },
        };
        unitOfWrite.NexusModsUserToSteamWorkshopMods.UpsertRange(nexusModsUserToSteamWorkshopMod);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return ApiResult("Allowed successful!");
    }

    [HttpDelete("SteamWorkshopModManualLinks")]
    public async Task<ApiResult<string?>> RemoveSteamWorkshopModManualLinkAsync([FromQuery, Required] SteamWorkshopModId modId, [FromQuery] NexusModsUserId? userId, [FromQuery] NexusModsUserName? username, CancellationToken ct)
    {
        var nexusModsUserId = await GetUserIdAsync(userId, username, ct);
        if (nexusModsUserId == NexusModsUserId.None)
            return ApiBadRequest("User not found!");

        var currentUserId = HttpContext.GetUserId();
        if (currentUserId != nexusModsUserId && HttpContext.GetRole() != ApplicationRoles.Moderator && HttpContext.GetRole() != ApplicationRoles.Administrator)
            return ApiBadRequest("Permission denied!");

        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite();

        unitOfWrite.NexusModsUserToSteamWorkshopMods
            .Remove(x => x.NexusModsUserId == nexusModsUserId && x.SteamWorkshopModId == modId && x.LinkType == NexusModsUserToModLinkType.ByOwner);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return ApiResult("Disallowed successful!");
    }

    [HttpPost("SteamWorkshopModManualLinks/Paginated")]
    public async Task<ApiResult<PagingData<UserManuallyLinkedSteamWorkshopModModel>?>> GetSteamWorkshopModManualLinkPaginatedAsync([FromBody] PaginatedQuery query, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();

        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var paginated = await unitOfRead.NexusModsUserToSteamWorkshopMods.GetManuallyLinkedPaginatedAsync(userId, query, ct);

        return ApiPagingResult(paginated);
    }
}