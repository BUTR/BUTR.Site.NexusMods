using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
public sealed class ModController : ControllerExtended
{
    public sealed record ModQuery(int ModId);

    public sealed record ManualLinkQuery(string ModId, int NexusModsId);
    public sealed record ManualUnlinkQuery(string ModId);

    public sealed record AllowModuleIdQuery(int UserId, string ModuleId);
    public sealed record DisallowModuleIdQuery(int UserId, string ModuleId);

    public sealed record AllowModQuery(int UserId, int ModId);
    public sealed record DisallowModQuery(int UserId, int ModId);



    private readonly ILogger _logger;
    private readonly NexusModsAPIClient _nexusModsAPIClient;
    private readonly AppDbContext _dbContext;
    private readonly NexusModsInfo _nexusModsInfo;

    public ModController(ILogger<ModController> logger, NexusModsAPIClient nexusModsAPIClient, AppDbContext dbContext, NexusModsInfo nexusModsInfo)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _nexusModsAPIClient = nexusModsAPIClient ?? throw new ArgumentNullException(nameof(nexusModsAPIClient));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _nexusModsInfo = nexusModsInfo ?? throw new ArgumentNullException(nameof(nexusModsInfo));
    }


    [HttpGet("RawGet/{gameDomain}/{modId:int}")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<RawModModel?>>> RawGetAsync(string gameDomain, int modId, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        var apiKey = HttpContext.GetAPIKey();

        if (await _nexusModsAPIClient.GetModAsync(gameDomain, modId, apiKey) is not { } modInfo)
            return APIResponseError<RawModModel>("Mod not found!");

        if (userId != modInfo.User.Id)
            return APIResponseError<RawModModel>("User does not have access to the mod!");

        return APIResponse(new RawModModel(modInfo.Name, modInfo.Id));
    }

    [HttpPost("ModPaginated")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<PagingData<ModModel>?>>> ModPaginatedAsync([FromBody] PaginatedQuery query, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();

        var manuallyLinkedUserIdsQuery =
            from userAllowedModuleIdsEntity in _dbContext.Set<NexusModsUserAllowedModuleIdsEntity>()
            from manuallyLinkedModuleIdEntity in _dbContext.Set<NexusModsModManualLinkedModuleIdEntity>()
            where userAllowedModuleIdsEntity.AllowedModuleIds.Contains(manuallyLinkedModuleIdEntity.ModuleId)
            select new
            {
                NexusModsUserId = userAllowedModuleIdsEntity.NexusModsUserId,
                NexusModsModId = manuallyLinkedModuleIdEntity.NexusModsModId
            };

        var mainQuery =
            from modEntity in _dbContext.Set<NexusModsModEntity>()
            where modEntity.UserIds.Contains(userId)
            join manuallyLinkedModuleIdEntity in _dbContext.Set<NexusModsModManualLinkedModuleIdEntity>()
                on modEntity.NexusModsModId equals manuallyLinkedModuleIdEntity.NexusModsModId
                into ManuallyLinkedModuleId
            join allowedUserIdsEntity in _dbContext.Set<NexusModsModManualLinkedNexusModsUsersEntity>()
                on modEntity.NexusModsModId equals allowedUserIdsEntity.NexusModsModId
                into AllowedUserIds
            from allowedUserIds in AllowedUserIds.DefaultIfEmpty()
            join exposedModsEntity in _dbContext.Set<NexusModsExposedModsEntity>()
                on modEntity.NexusModsModId equals exposedModsEntity.NexusModsModId
                into ExposedMods
            from exposedModEntity in ExposedMods.DefaultIfEmpty()
                //join manuallyLinkedUserIdsEntity in manuallyLinkedUserIdsQuery
                //    on modEntity.NexusModsModId equals manuallyLinkedUserIdsEntity.NexusModsModId
                //    into ManuallyLinkedUserId
                //from manuallyLinkedUserId in ManuallyLinkedUserId.DefaultIfEmpty()
            select new
            {
                NexusModsModId = modEntity.NexusModsModId,
                Name = modEntity.Name,
                UserIds = modEntity.UserIds,
                ManuallyLinkedModuleIds = ManuallyLinkedModuleId.Select(x => x.ModuleId).ToArray(),
                AllowedNexusModsUserIds = allowedUserIds.AllowedNexusModsUserIds,
                KnownModuleIds = exposedModEntity.ModuleIds,
                //ManuallyLinkedUserIds = manuallyLinkedUserId,
            };
        var paginated = await mainQuery
            .PaginatedAsync(query, 20, new() { Property = nameof(NexusModsModEntity.NexusModsModId), Type = SortingType.Ascending }, ct);

        var nexusModsModIds = await paginated.Items.Select(x => x.NexusModsModId).ToArrayAsync(ct);
        var manuallyLinkedUserIds = await manuallyLinkedUserIdsQuery
            .Where(x => nexusModsModIds.Contains(x.NexusModsModId))
            .GroupBy(x => x.NexusModsModId, x => x.NexusModsUserId)
            .ToDictionaryAsync(x => x.Key, x => x.ToArray(), ct);

        return APIPagingResponse(paginated, items => items.Select(m => new ModModel(
            m.Name,
            m.NexusModsModId,
            m.AllowedNexusModsUserIds?.AsImmutableArray() ?? ImmutableArray<int>.Empty,
            //ImmutableArray<int>.Empty,
            //m.ManuallyLinkedUserIds?.AsImmutableArray() ?? ImmutableArray<int>.Empty,
            manuallyLinkedUserIds.TryGetValue(m.NexusModsModId, out var arr) ? arr.AsImmutableArray() : ImmutableArray<int>.Empty,
            m.ManuallyLinkedModuleIds?.AsImmutableArray() ?? ImmutableArray<string>.Empty,
            m.KnownModuleIds?.AsImmutableArray() ?? ImmutableArray<string>.Empty)));
    }

    [HttpPost("ModUpdate")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> ModUpdateAsync([FromBody] ModQuery query)
    {
        var userId = HttpContext.GetUserId();
        var apiKey = HttpContext.GetAPIKey();

        if (await _nexusModsAPIClient.GetModAsync("mountandblade2bannerlord", query.ModId, apiKey) is not { } modInfo)
            return APIResponseError<string>("Mod not found!");

        if (userId != modInfo.User.Id)
            return APIResponseError<string>("User does not have access to the mod!");

        NexusModsModEntity? ApplyChanges(NexusModsModEntity? existing) => existing switch
        {
            null => null,
            _ => existing with { Name = modInfo.Name }
        };
        if (await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsModEntity>(x => x.NexusModsModId == query.ModId, ApplyChanges))
            return APIResponse("Updated successful!");

        return APIResponseError<string>("Failed to link the mod!");
    }


    [HttpGet("ModLink")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> ModLinkAsync([FromQuery] ModQuery query)
    {
        var userId = HttpContext.GetUserId();
        var apiKey = HttpContext.GetAPIKey();

        if (await _nexusModsAPIClient.GetModAsync("mountandblade2bannerlord", query.ModId, apiKey) is not { } modInfo)
            return APIResponseError<string>("Mod not found!");

        if (userId != modInfo.User.Id)
            return APIResponseError<string>("User does not have access to the mod!");

        if (HttpContext.GetIsPremium()) // Premium is needed for API based downloading
        {
            var exposedModIds = await _nexusModsInfo.GetModIdsAsync("mountandblade2bannerlord", modInfo.Id, apiKey).Distinct().ToImmutableArrayAsync();
            NexusModsExposedModsEntity? ApplyChanges2(NexusModsExposedModsEntity? existing) => existing switch
            {
                null => new() { NexusModsModId = modInfo.Id, ModuleIds = exposedModIds.AsArray(), LastCheckedDate = DateTime.UtcNow },
                _ => existing with { ModuleIds = existing.ModuleIds.AsImmutableArray().AddRange(exposedModIds.Except(existing.ModuleIds)).AsArray(), LastCheckedDate = DateTime.UtcNow }
            };
            if (!await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsExposedModsEntity>(x => x.NexusModsModId == query.ModId, ApplyChanges2))
                return APIResponseError<string>("Failed to link!");
        }

        NexusModsModEntity? ApplyChanges(NexusModsModEntity? existing) => existing switch
        {
            null => new() { Name = modInfo.Name, NexusModsModId = modInfo.Id, UserIds = ImmutableArray.Create<int>(userId).AsArray() },
            _ when existing.UserIds.Contains(userId) => existing,
            _ => existing with { UserIds = ImmutableArray.Create<int>(userId).AsArray() }
        };
        if (await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsModEntity>(x => x.NexusModsModId == query.ModId, ApplyChanges))
            return APIResponse("Linked successful!");

        return APIResponseError<string>("Failed to link!");
    }

    [HttpGet("ModUnlink")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> ModUnlinkAsync([FromQuery] ModQuery query)
    {
        var userId = HttpContext.GetUserId();

        NexusModsModEntity? ApplyChanges(NexusModsModEntity? existing) => existing switch
        {
            null => null,
            _ when existing.UserIds.Contains(userId) && existing.UserIds.Length == 1 => null,
            _ when !existing.UserIds.Contains(userId) => existing,
            _ => existing with { UserIds = existing.UserIds.AsImmutableArray().Remove(userId).AsArray() }
        };
        if (await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsModEntity>(x => x.NexusModsModId == query.ModId, ApplyChanges))
            return APIResponse("Unlinked successful!");

        return APIResponseError<string>("Failed to unlink!");
    }


    [HttpGet("ModManualLink")]
    [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> ModManualLinkAsync([FromQuery] ManualLinkQuery query)
    {
        NexusModsModManualLinkedModuleIdEntity? ApplyChanges(NexusModsModManualLinkedModuleIdEntity? existing) => existing switch
        {
            null => new() { ModuleId = query.ModId, NexusModsModId = query.NexusModsId },
            _ => existing with { NexusModsModId = query.NexusModsId }
        };
        if (await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsModManualLinkedModuleIdEntity>(x => x.ModuleId == query.ModId, ApplyChanges))
            return APIResponse("Linked successful!");

        return APIResponseError<string>("Failed to link!");
    }

    [HttpGet("ModManualUnlink")]
    [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> ModManualUnlinkAsync([FromQuery] ManualUnlinkQuery query)
    {
        NexusModsModManualLinkedModuleIdEntity? ApplyChanges(NexusModsModManualLinkedModuleIdEntity? existing) => existing switch
        {
            _ => null
        };
        if (await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsModManualLinkedModuleIdEntity>(x => x.ModuleId == query.ModId, ApplyChanges))
            return APIResponse("Unlinked successful!");

        return APIResponseError<string>("Failed to unlink!");
    }

    [HttpPost("ModManualLinkPaginated")]
    [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<PagingData<ModNexusModsManualLinkModel>?>>> ModManualLinkPaginatedAsync([FromBody] PaginatedQuery query, CancellationToken ct)
    {
        var paginated = await _dbContext.Set<NexusModsModManualLinkedModuleIdEntity>()
            .PaginatedAsync(query, 20, new() { Property = nameof(NexusModsModManualLinkedModuleIdEntity.ModuleId), Type = SortingType.Ascending }, ct);

        return APIPagingResponse(paginated, items => items.Select(m => new ModNexusModsManualLinkModel(m.ModuleId, m.NexusModsModId)));
    }


    [HttpGet("AllowUserAModuleId")]
    [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> AllowUserAModuleIdAsync([FromQuery] AllowModuleIdQuery query)
    {
        NexusModsUserAllowedModuleIdsEntity? ApplyChanges(NexusModsUserAllowedModuleIdsEntity? existing) => existing switch
        {
            null => new() { NexusModsUserId = query.UserId, AllowedModuleIds = ImmutableArray.Create<string>(query.ModuleId).AsArray() },
            _ when existing.AllowedModuleIds.Contains(query.ModuleId) => existing,
            _ => existing with { AllowedModuleIds = existing.AllowedModuleIds.AsImmutableArray().Add(query.ModuleId).AsArray() }
        };
        if (await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsUserAllowedModuleIdsEntity>(x => x.NexusModsUserId == query.UserId, ApplyChanges))
            return APIResponse("Allowed successful!");

        return APIResponseError<string>("Failed to allowed!");
    }

    [HttpGet("DisallowUserAModuleId")]
    [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> DisallowUserAModuleIdAsync([FromQuery] DisallowModuleIdQuery query)
    {
        NexusModsUserAllowedModuleIdsEntity? ApplyChanges(NexusModsUserAllowedModuleIdsEntity? existing) => existing switch
        {
            null => null,
            _ when existing.AllowedModuleIds.Contains(query.ModuleId) && existing.AllowedModuleIds.Length == 1 => null,
            _ when !existing.AllowedModuleIds.Contains(query.ModuleId) => existing,
            _ => existing with { AllowedModuleIds = existing.AllowedModuleIds.AsImmutableArray().Remove(query.ModuleId).AsArray() }
        };
        if (await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsUserAllowedModuleIdsEntity>(x => x.NexusModsUserId == query.UserId, ApplyChanges))
            return APIResponse("Disallowed successful!");

        return APIResponseError<string>("Failed to disallowed!");
    }

    [HttpPost("AllowUserAModuleIdPaginated")]
    [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<PagingData<UserAllowedModuleIdsModel>?>>> AllowUserAModuleIdPaginatedAsync([FromBody] PaginatedQuery query, CancellationToken ct)
    {
        var paginated = await _dbContext.Set<NexusModsUserAllowedModuleIdsEntity>()
            .PaginatedAsync(query, 20, new() { Property = nameof(NexusModsUserAllowedModuleIdsEntity.NexusModsUserId), Type = SortingType.Ascending }, ct);

        return APIPagingResponse(paginated, items => items.Select(m => new UserAllowedModuleIdsModel(m.NexusModsUserId, m.AllowedModuleIds.AsImmutableArray())));
    }


    [HttpGet("AllowUserAMod")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> AllowUserAModAsync([FromQuery] AllowModQuery query)
    {
        var userId = HttpContext.GetUserId();
        var apiKey = HttpContext.GetAPIKey();

        if (await _nexusModsAPIClient.GetModAsync("mountandblade2bannerlord", query.ModId, apiKey) is not { } modInfo)
            return APIResponseError<string>("Mod not found!");

        if (userId != modInfo.User.Id)
            return APIResponseError<string>("User does not have access to the mod!");


        NexusModsModManualLinkedNexusModsUsersEntity? ApplyChanges(NexusModsModManualLinkedNexusModsUsersEntity? existing) => existing switch
        {
            null => new() { NexusModsModId = query.ModId, AllowedNexusModsUserIds = ImmutableArray.Create<int>(query.UserId).AsArray() },
            _ when existing.AllowedNexusModsUserIds.Contains(query.UserId) => existing,
            _ => existing with { AllowedNexusModsUserIds = existing.AllowedNexusModsUserIds.AsImmutableArray().Add(query.UserId).AsArray() }
        };
        if (await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsModManualLinkedNexusModsUsersEntity>(x => x.NexusModsModId == query.ModId, ApplyChanges))
            return APIResponse("Allowed successful!");

        return APIResponseError<string>("Failed to allowed!");
    }

    [HttpGet("DisallowUserAMod")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> DisallowUserAModAsync([FromQuery] DisallowModQuery query)
    {
        NexusModsModManualLinkedNexusModsUsersEntity? ApplyChanges(NexusModsModManualLinkedNexusModsUsersEntity? existing) => existing switch
        {
            null => null,
            _ when existing.AllowedNexusModsUserIds.Contains(query.UserId) && existing.AllowedNexusModsUserIds.Length == 1 => null,
            _ when !existing.AllowedNexusModsUserIds.Contains(query.UserId) => existing,
            _ => existing with { AllowedNexusModsUserIds = existing.AllowedNexusModsUserIds.AsImmutableArray().Remove(query.UserId).AsArray() }
        };
        if (await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsModManualLinkedNexusModsUsersEntity>(x => x.NexusModsModId == query.ModId, ApplyChanges))
            return APIResponse("Disallowed successful!");

        return APIResponseError<string>("Failed to disallowed!");
    }

    [HttpPost("AllowUserAModPaginated")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<PagingData<UserAllowedModsModel>?>>> AllowUserAModPaginatedAsync([FromBody] PaginatedQuery query, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();

        var ownedModIs = await _dbContext.Set<NexusModsModEntity>()
            .Where(y => y.UserIds.Contains(userId))
            .Select(x => x.NexusModsModId)
            .ToArrayAsync(ct);

        var paginated = await _dbContext.Set<NexusModsModManualLinkedNexusModsUsersEntity>()
            .Where(x => ownedModIs.Contains(x.NexusModsModId))
            .PaginatedAsync(query, 20, new() { Property = nameof(NexusModsModManualLinkedNexusModsUsersEntity.NexusModsModId), Type = SortingType.Ascending }, ct);

        return APIPagingResponse(paginated, items => items.Select(m => new UserAllowedModsModel(m.NexusModsModId, m.AllowedNexusModsUserIds.AsImmutableArray())));
    }


    [HttpPost("AvailableModsPaginated")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<PagingData<AvailableModModel>?>>> AvailableModsPaginatedAsync([FromBody] PaginatedQuery query, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();

        // Can't join because ARRAY CONTAINS
        var firstQuery =
            from userAllowedModuleIdsEntity in _dbContext.Set<NexusModsUserAllowedModuleIdsEntity>()
            from manuallyLinkedModuleIdEntity in _dbContext.Set<NexusModsModManualLinkedModuleIdEntity>()
            where userAllowedModuleIdsEntity.AllowedModuleIds.Contains(manuallyLinkedModuleIdEntity.ModuleId)
            where userAllowedModuleIdsEntity.NexusModsUserId == userId
            select new
            {
                NexusModsModId = manuallyLinkedModuleIdEntity.NexusModsModId
            };

        var secondQuery =
            from modManualLinkedNexusModsUsersEntity in _dbContext.Set<NexusModsModManualLinkedNexusModsUsersEntity>()
            where modManualLinkedNexusModsUsersEntity.AllowedNexusModsUserIds.Contains(userId)
            select new
            {
                NexusModsModId = modManualLinkedNexusModsUsersEntity.NexusModsModId
            };

        var mainQuery =
            from nexusModsModId in firstQuery.Union(secondQuery).Select(x => x.NexusModsModId)
            join modEntity in _dbContext.Set<NexusModsModEntity>()
                on nexusModsModId equals modEntity.NexusModsModId
            select modEntity;

        var paginated = await mainQuery
            .PaginatedAsync(query, 20, new() { Property = nameof(NexusModsModEntity.NexusModsModId), Type = SortingType.Ascending }, ct);

        return APIPagingResponse(paginated, items => items.Select(x => new AvailableModModel(x.NexusModsModId, x.Name)));
    }
}