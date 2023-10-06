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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
public sealed class NexusModsUserController : ControllerExtended
{
    public sealed record SetRoleBody(NexusModsUserId NexusModsUserId, ApplicationRole Role);

    public sealed record RemoveRoleBody(NexusModsUserId NexusModsUserId);

    public sealed record NexusModsUserToNexusModsModQuery(NexusModsModId NexusModsModId);

    public sealed record NexusModsUserToModuleQuery(NexusModsUserId NexusModsUserId, ModuleId ModuleId);

    public sealed record NexusModsUserToNexusModsModManualLinkQuery(NexusModsUserId NexusModsUserId, NexusModsModId NexusModsModId);

    public sealed record NexusModsUserToModuleManualLinkModel(NexusModsUserId NexusModsUserId, ImmutableArray<ModuleId> AllowedModuleIds);

    public sealed record NexusModsUserToNexusModsModManualLinkModel(NexusModsModId NexusModsModId, ImmutableArray<NexusModsUserId> AllowedNexusModsUserIds);

    private readonly ILogger _logger;
    private readonly NexusModsAPIClient _nexusModsAPIClient;
    private readonly NexusModsModFileParser _nexusModsModFileParser;
    private readonly IAppDbContextWrite _dbContextWrite;
    private readonly IAppDbContextRead _dbContextRead;

    public NexusModsUserController(ILogger<NexusModsUserController> logger, NexusModsAPIClient nexusModsAPIClient, NexusModsModFileParser nexusModsModFileParser, IAppDbContextWrite dbContextWrite, IAppDbContextRead dbContextRead)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _nexusModsAPIClient = nexusModsAPIClient ?? throw new ArgumentNullException(nameof(nexusModsAPIClient));
        _nexusModsModFileParser = nexusModsModFileParser ?? throw new ArgumentNullException(nameof(nexusModsModFileParser));
        _dbContextWrite = dbContextWrite ?? throw new ArgumentNullException(nameof(dbContextWrite));
        _dbContextRead = dbContextRead ?? throw new ArgumentNullException(nameof(dbContextRead));
    }


    [HttpGet("Profile")]
    [Produces("application/json")]
    public ActionResult<APIResponse<ProfileModel?>> Profile() => APIResponse(HttpContext.GetProfile());


    [HttpPost("SetRole")]
    [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> SetRoleAsync([FromQuery] SetRoleBody body, CancellationToken ct)
    {
        await using var _ = _dbContextWrite.CreateSaveScope();

        var tenant = HttpContext.GetTenant();
        if (tenant == TenantId.None) return BadRequest();

        _dbContextWrite.FutureUpsert(x => x.NexusModsUsers, NexusModsUserEntity.CreateWithRole(body.NexusModsUserId, tenant, body.Role));
        return APIResponse("Set successful!");
    }

    [HttpDelete("RemoveRole")]
    [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> RemoveRoleAsync([FromQuery] RemoveRoleBody body, CancellationToken ct)
    {
        await using var _ = _dbContextWrite.CreateSaveScope();

        var tenant = HttpContext.GetTenant();
        if (tenant == TenantId.None) return BadRequest();

        _dbContextWrite.FutureUpsert(x => x.NexusModsUsers, NexusModsUserEntity.CreateWithRole(body.NexusModsUserId, tenant, ApplicationRole.User));
        return APIResponse("Removed successful!");
    }

    [HttpPost("ToNexusModsModPaginated")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<PagingData<NexusModsModModel>?>>> ToNexusModsModPaginatedAsync([FromBody] PaginatedQuery query, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();

       var availableModsByNexusModsModLinkage = _dbContextRead.NexusModsUsers
            .Include(x => x.ToNexusModsMods)
            .ThenInclude(x => x.NexusModsMod)
            .ThenInclude(x => x.ToNexusModsUsers)
            .ThenInclude(x => x.NexusModsUser)
            .AsSplitQuery()
            .Where(x => x.NexusModsUserId == userId)
            .SelectMany(x => x.ToNexusModsMods)
            .Select(x => x.NexusModsMod)
            .Select(x => new
            {
                NexusModsModId = x.NexusModsModId,
                Name = x.Name!.Name,
                OwnerNexusModsUserIds = x.ToNexusModsUsers.Where(y => y.NexusModsUser.NexusModsUserId != userId && y.LinkType == NexusModsUserToNexusModsModLinkType.ByAPIConfirmation).Select(y => y.NexusModsUser.NexusModsUserId).ToArray(),
                AllowedNexusModsUserIds = x.ToNexusModsUsers.Where(y => y.NexusModsUser.NexusModsUserId != userId && y.LinkType == NexusModsUserToNexusModsModLinkType.ByOwner || y.LinkType == NexusModsUserToNexusModsModLinkType.ByStaff).Select(y => y.NexusModsUser.NexusModsUserId).ToArray(),
                NexusModsUserIds = x.ToNexusModsUsers.Where(y => y.NexusModsUser.NexusModsUserId != userId && y.LinkType == NexusModsUserToNexusModsModLinkType.ByOwner).Select(y => y.NexusModsUser.NexusModsUserId).ToArray(),
                LinkedModuleIds = x.ModuleIds.Where(y => y.LinkType == NexusModsModToModuleLinkType.ByStaff).Select(y => y.Module.ModuleId).ToArray(),
                KnownModuleIds = x.ModuleIds.Where(y => y.LinkType == NexusModsModToModuleLinkType.ByUnverifiedFileExposure).Select(y => y.Module.ModuleId).ToArray(),
            });

        var paginated = await availableModsByNexusModsModLinkage
            .PaginatedAsync(query, 20, new() { Property = nameof(NexusModsModEntity.NexusModsModId), Type = SortingType.Ascending }, ct);

        return APIPagingResponse(paginated, items => items.Select(m => new NexusModsModModel(
            NexusModsModId: m.NexusModsModId,
            Name: m.Name,
            AllowedNexusModsUserIds: m.AllowedNexusModsUserIds.AsImmutableArray(),
            ManuallyLinkedNexusModsUserIds:  m.NexusModsUserIds.AsImmutableArray(),
            ManuallyLinkedModuleIds: m.LinkedModuleIds.AsImmutableArray(),
            KnownModuleIds: m.KnownModuleIds.AsImmutableArray())));
    }

    [HttpPost("ToNexusModsModUpdate")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> ToNexusModsModUpdateAsync([FromBody] NexusModsUserToNexusModsModQuery query, CancellationToken ct)
    {
        await using var _ = _dbContextWrite.CreateSaveScope();
        var entityFactory = _dbContextWrite.CreateEntityFactory();

        var userId = HttpContext.GetUserId();
        var apiKey = HttpContext.GetAPIKey();
        var tenant = HttpContext.GetTenant();
        if (tenant == TenantId.None) return BadRequest();

        var gameDomain = tenant.ToGameDomain();

        if (await _nexusModsAPIClient.GetModAsync(gameDomain, query.NexusModsModId, apiKey, ct) is not { } modInfo)
            return APIResponseError<string>("Mod not found!");

        if (userId != modInfo.User.Id)
            return APIResponseError<string>("User does not have access to the mod!");

        var entity = new NexusModsModToNameEntity
        {
            TenantId = tenant,
            NexusModsMod = entityFactory.GetOrCreateNexusModsMod(query.NexusModsModId),
            Name = modInfo.Name,
        };
        _dbContextWrite.FutureUpsert(x => x.NexusModsModName, entity);
        return APIResponse("Updated successful!");
    }


    [HttpGet("ToNexusModsModLink")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> ToNexusModsModLinkAsync([FromQuery] NexusModsUserToNexusModsModQuery query, CancellationToken ct)
    {
        await using var _ = _dbContextWrite.CreateSaveScope();
        var entityFactory = _dbContextWrite.CreateEntityFactory();

        var userId = HttpContext.GetUserId();
        var apiKey = HttpContext.GetAPIKey();
        var tenant = HttpContext.GetTenant();
        if (tenant == TenantId.None) return BadRequest();

        var gameDomain = tenant.ToGameDomain();

        if (await _nexusModsAPIClient.GetModAsync(gameDomain, query.NexusModsModId, apiKey, ct) is not { } modInfo)
            return APIResponseError<string>("Mod not found!");

        if (userId != modInfo.User.Id)
            return APIResponseError<string>("User does not have access to the mod!");

        if (HttpContext.GetIsPremium()) // Premium is needed for API based downloading
        {
            var exposedModIds = await _nexusModsModFileParser.GetModuleInfosAsync(gameDomain, modInfo.Id, apiKey, ct).Distinct().ToImmutableArrayAsync(ct);
            var entities = exposedModIds.Select(y => new NexusModsModToModuleEntity
            {
                TenantId = tenant,
                NexusModsMod = entityFactory.GetOrCreateNexusModsMod(query.NexusModsModId),
                Module = entityFactory.GetOrCreateModule(ModuleId.From(y.Id)),
                LinkType = NexusModsModToModuleLinkType.ByUnverifiedFileExposure,
                LastUpdateDate = DateTime.UtcNow
            }).ToList();
            _dbContextWrite.FutureUpsert(x => x.NexusModsModModules, entities);
        }

        var nexusModsModToName = new NexusModsModToNameEntity
        {
            TenantId = tenant,
            NexusModsMod = entityFactory.GetOrCreateNexusModsMod(query.NexusModsModId),
            Name = modInfo.Name,
        };
        _dbContextWrite.FutureUpsert(x => x.NexusModsModName, nexusModsModToName);
        var nexusModsUserToNexusModsMod = new NexusModsUserToNexusModsModEntity
        {
            TenantId = tenant,
            NexusModsUser = entityFactory.GetOrCreateNexusModsUser(userId),
            NexusModsMod = entityFactory.GetOrCreateNexusModsMod(query.NexusModsModId),
            LinkType = NexusModsUserToNexusModsModLinkType.ByAPIConfirmation,
        };
        _dbContextWrite.FutureUpsert(x => x.NexusModsUserToNexusModsMods, nexusModsUserToNexusModsMod);
        return APIResponse("Linked successful!");
    }

    [HttpGet("ToNexusModsModUnlink")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> ToNexusModsModUnlinkAsync([FromQuery] NexusModsUserToNexusModsModQuery query)
    {
        var userId = HttpContext.GetUserId();

        await _dbContextWrite.NexusModsUserToNexusModsMods
            .Where(x => x.NexusModsUser.NexusModsUserId == userId && x.NexusModsMod.NexusModsModId == query.NexusModsModId && x.LinkType == NexusModsUserToNexusModsModLinkType.ByOwner)
            .ExecuteDeleteAsync();
        return APIResponse("Unlinked successful!");
    }


    [HttpGet("ToModuleManualLink")]
    [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> ToModuleManualLinkAsync([FromQuery] NexusModsUserToModuleQuery query)
    {
        await using var _ = _dbContextWrite.CreateSaveScope();
        var entityFactory = _dbContextWrite.CreateEntityFactory();

        var tenant = HttpContext.GetTenant();
        if (tenant == TenantId.None) return BadRequest();

        var nexusModsUserToModule = new NexusModsUserToModuleEntity
        {
            TenantId = tenant,
            NexusModsUser = entityFactory.GetOrCreateNexusModsUser(query.NexusModsUserId),
            Module = entityFactory.GetOrCreateModule(query.ModuleId),
            LinkType = NexusModsUserToModuleLinkType.ByStaff,
        };
        _dbContextWrite.FutureUpsert(x => x.NexusModsUserToModules, nexusModsUserToModule);
        return APIResponse("Allowed successful!");
    }

    [HttpGet("ToModuleManualUnlink")]
    [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> ToModuleManualUnlinkAsync([FromQuery] NexusModsUserToModuleQuery query)
    {
        await _dbContextWrite.NexusModsUserToModules
            .Where(x => x.NexusModsUser.NexusModsUserId == query.NexusModsUserId && x.Module.ModuleId == query.ModuleId && x.LinkType == NexusModsUserToModuleLinkType.ByStaff)
            .DeleteFromQueryAsync();
        return APIResponse("Disallowed successful!");
    }

    [HttpPost("ToModuleManualLinkPaginated")]
    [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<PagingData<NexusModsUserToModuleManualLinkModel>?>>> ToModuleManualLinkPaginatedAsync([FromBody] PaginatedQuery query, CancellationToken ct)
    {
        var paginated = await _dbContextRead.NexusModsUserToModules
            .Where(x => x.LinkType == NexusModsUserToModuleLinkType.ByStaff)
            .GroupBy(x =>  new { x.NexusModsUser.NexusModsUserId })
            .Select(x => new { NexusModsUserId = x.Key.NexusModsUserId, ModuleIds = x.Select(y => y.Module.ModuleId).ToArray() })
            .PaginatedAsync(query, 20, new() { Property = nameof(NexusModsUserEntity.NexusModsUserId), Type = SortingType.Ascending }, ct);

        return APIPagingResponse(paginated, items => items.Select(m => new NexusModsUserToModuleManualLinkModel(m.NexusModsUserId, m.ModuleIds.AsImmutableArray())));
    }


    [HttpGet("ToNexusModsModManualLink")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> ToNexusModsModManualLinkAsync([FromQuery] NexusModsUserToNexusModsModManualLinkQuery query, CancellationToken ct)
    {
        await using var _ = _dbContextWrite.CreateSaveScope();
        var entityFactory = _dbContextWrite.CreateEntityFactory();

        var userId = HttpContext.GetUserId();
        var apiKey = HttpContext.GetAPIKey();
        var tenant = HttpContext.GetTenant();
        if (tenant == TenantId.None) return BadRequest();

        var gameDomain = tenant.ToGameDomain();

        if (await _nexusModsAPIClient.GetModAsync(gameDomain, query.NexusModsModId, apiKey, ct) is not { } modInfo)
            return APIResponseError<string>("Mod not found!");

        if (userId != modInfo.User.Id)
            return APIResponseError<string>("User does not have access to the mod!");


        var nexusModsModToName = new NexusModsModToNameEntity
        {
            TenantId = tenant,
            NexusModsMod = entityFactory.GetOrCreateNexusModsMod(query.NexusModsModId),
            Name = modInfo.Name,
        };
        _dbContextWrite.FutureUpsert(x => x.NexusModsModName, nexusModsModToName);
        var nexusModsUserToNexusModsMods = new List<NexusModsUserToNexusModsModEntity>
        {
            new()
            {
                TenantId = tenant,
                NexusModsUser = entityFactory.GetOrCreateNexusModsUser(userId),
                NexusModsMod = entityFactory.GetOrCreateNexusModsMod(query.NexusModsModId),
                LinkType = NexusModsUserToNexusModsModLinkType.ByAPIConfirmation,
            },
            new()
            {
                TenantId = tenant,
                NexusModsUser = entityFactory.GetOrCreateNexusModsUser(query.NexusModsUserId),
                NexusModsMod = entityFactory.GetOrCreateNexusModsMod(query.NexusModsModId),
                LinkType = NexusModsUserToNexusModsModLinkType.ByOwner,
            }
        };
        _dbContextWrite.FutureUpsert(x => x.NexusModsUserToNexusModsMods, nexusModsUserToNexusModsMods);
        return APIResponse("Allowed successful!");
    }

    [HttpGet("ToNexusModsModManualUnlink")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> ToNexusModsModManualUnlinkAsync([FromQuery] NexusModsUserToNexusModsModManualLinkQuery query)
    {
        await _dbContextWrite.NexusModsUserToNexusModsMods
            .Where(x => x.NexusModsUser.NexusModsUserId == query.NexusModsUserId && x.NexusModsMod.NexusModsModId == query.NexusModsModId && x.LinkType == NexusModsUserToNexusModsModLinkType.ByOwner)
            .ExecuteDeleteAsync();
        return APIResponse("Disallowed successful!");
    }

    [HttpPost("ToNexusModsModManualLinkPaginated")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<PagingData<NexusModsUserToNexusModsModManualLinkModel>?>>> ToNexusModsModManualLinkPaginatedAsync([FromBody] PaginatedQuery query, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();

        var paginated = await _dbContextRead.NexusModsUserToNexusModsMods
            .Where(x => x.NexusModsUser.NexusModsUserId == userId && x.LinkType == NexusModsUserToNexusModsModLinkType.ByOwner)
            .GroupBy(x =>  new { NexusModsModId = x.NexusModsMod.NexusModsModId })
            .Select(x => new { NexusModsModId = x.Key.NexusModsModId, NexusModsUserIds = x.Select(y => y.NexusModsUser.NexusModsUserId).ToArray() })
            .PaginatedAsync(query, 20, new() { Property = nameof(NexusModsModEntity.NexusModsModId), Type = SortingType.Ascending }, ct);

        return APIPagingResponse(paginated, items => items.Select(m => new NexusModsUserToNexusModsModManualLinkModel(m.NexusModsModId, m.NexusModsUserIds.AsImmutableArray())));
    }
}