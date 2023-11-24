using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.APIResponses;
using BUTR.Site.NexusMods.Server.Utils.BindingSources;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), ButrNexusModsAuthorization, TenantRequired]
public sealed class NexusModsModController : ControllerExtended
{
    public sealed record RawNexusModsModModel(NexusModsModId NexusModsModId, string Name);

    public sealed record NexusModsModToModuleModel(ModuleId ModuleId, NexusModsModId NexusModsModId);

    public sealed record NexusModsModToModuleManualLinkQuery(ModuleId ModuleId, NexusModsModId NexusModsModId);

    public sealed record NexusModsModAvailableModel(NexusModsModId NexusModsModId, string Name);


    private readonly ILogger _logger;
    private readonly NexusModsAPIClient _nexusModsAPIClient;
    private readonly IAppDbContextRead _dbContextRead;
    private readonly IAppDbContextWrite _dbContextWrite;

    public NexusModsModController(ILogger<NexusModsModController> logger, NexusModsAPIClient nexusModsAPIClient, IAppDbContextRead dbContextRead, IAppDbContextWrite dbContextWrite)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _nexusModsAPIClient = nexusModsAPIClient ?? throw new ArgumentNullException(nameof(nexusModsAPIClient));
        _dbContextRead = dbContextRead ?? throw new ArgumentNullException(nameof(dbContextRead));
        _dbContextWrite = dbContextWrite ?? throw new ArgumentNullException(nameof(dbContextWrite));
    }


    [HttpGet("Raw/{gameDomain}/{modId}")]
    [Produces("application/json")]
    public async Task<APIResponseActionResult<RawNexusModsModModel?>> RawAsync(NexusModsGameDomain gameDomain, NexusModsModId modId, [BindApiKey] NexusModsApiKey apiKey, [BindUserId] NexusModsUserId userId, CancellationToken ct)
    {
        if (await _nexusModsAPIClient.GetModAsync(gameDomain, modId, apiKey, ct) is not { } modInfo)
            return APIResponseError<RawNexusModsModModel>("Mod not found!");

        if (userId != modInfo.User.Id)
            return APIResponseError<RawNexusModsModModel>("User does not have access to the mod!");

        return APIResponse(new RawNexusModsModModel(modInfo.Id, modInfo.Name));
    }


    [HttpGet("ToModuleManualLink")]
    [ButrNexusModsAuthorization(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    [Produces("application/json")]
    public async Task<APIResponseActionResult<string?>> ToModuleManualLinkAsync([FromQuery] NexusModsModToModuleManualLinkQuery query, [BindTenant] TenantId tenant)
    {
        await using var _ = await _dbContextWrite.CreateSaveScopeAsync();
        var entityFactory = _dbContextWrite.GetEntityFactory();

        var nexusModsModToModule = new NexusModsModToModuleEntity
        {
            TenantId = tenant,
            NexusModsMod = entityFactory.GetOrCreateNexusModsMod(query.NexusModsModId),
            Module = entityFactory.GetOrCreateModule(query.ModuleId),
            LinkType = NexusModsModToModuleLinkType.ByStaff,
            LastUpdateDate = DateTimeOffset.UtcNow,
        };
        await _dbContextWrite.NexusModsModModules.UpsertOnSaveAsync(nexusModsModToModule);
        return APIResponse("Linked successful!");
    }

    [HttpGet("ToModuleManualUnlink")]
    [ButrNexusModsAuthorization(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    [Produces("application/json")]
    public async Task<APIResponseActionResult<string?>> ToModuleManualUnlinkAsync([FromQuery] NexusModsModToModuleManualLinkQuery query)
    {
        await _dbContextWrite.NexusModsModModules
            .Where(x => x.Module.ModuleId == query.ModuleId && x.NexusModsMod.NexusModsModId == query.NexusModsModId && x.LinkType == NexusModsModToModuleLinkType.ByStaff)
            .ExecuteDeleteAsync();

        return APIResponseError<string>("Failed to unlink!");
    }

    [HttpPost("ToModuleManualLinkPaginated")]
    [ButrNexusModsAuthorization(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    [Produces("application/json")]
    public async Task<APIResponseActionResult<PagingData<NexusModsModToModuleModel>?>> ToModuleManualLinkPaginatedAsync([FromBody] PaginatedQuery query, CancellationToken ct)
    {
        var paginated = await _dbContextRead.NexusModsModModules
            .Where(x => x.LinkType == NexusModsModToModuleLinkType.ByStaff)
            .PaginatedAsync(query, 20, new() { Property = nameof(ModuleEntity.ModuleId), Type = SortingType.Ascending }, ct);

        return APIPagingResponse(paginated, items => items.Select(m => new NexusModsModToModuleModel(m.Module.ModuleId, m.NexusModsMod.NexusModsModId)));
    }


    [HttpPost("AvailablePaginated")]
    [Produces("application/json")]
    public async Task<APIResponseActionResult<PagingData<NexusModsModAvailableModel>?>> AvailablePaginatedAsync([FromBody] PaginatedQuery query, [BindUserId] NexusModsUserId userId, CancellationToken ct)
    {
        var userToModIds = _dbContextRead.NexusModsUserToNexusModsMods
            .Include(x => x.NexusModsMod).ThenInclude(x => x.Name)
            .Where(x => x.NexusModsUser.NexusModsUserId == userId)
            .Select(x => x.NexusModsMod);

        var userToModuleIdsToModIds = _dbContextRead.NexusModsUserToModules
            .Include(x => x.Module).ThenInclude(x => x.ToNexusModsMods).ThenInclude(x => x.NexusModsMod).ThenInclude(x => x.Name)
            .Where(x => x.NexusModsUser.NexusModsUserId == userId)
            .Select(x => x.Module)
            .SelectMany(x => x.ToNexusModsMods)
            .Select(x => x.NexusModsMod);

        var paginated = await userToModIds.Union(userToModuleIdsToModIds)
            .PaginatedAsync(query, 20, new() { Property = nameof(NexusModsModEntity.NexusModsModId), Type = SortingType.Ascending }, ct);

        return APIPagingResponse(paginated, items => items.Select(x => new NexusModsModAvailableModel(x.NexusModsModId, x.Name!.Name)));
    }
}