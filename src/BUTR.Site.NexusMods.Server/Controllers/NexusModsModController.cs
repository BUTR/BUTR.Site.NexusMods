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

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), ButrNexusModsAuthorization, TenantRequired]
public sealed class NexusModsModController : ApiControllerBase
{
    public sealed record RawNexusModsModModel(NexusModsModId NexusModsModId, string Name);


    private readonly ILogger _logger;
    private readonly INexusModsAPIClient _nexusModsAPIClient;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public NexusModsModController(ILogger<NexusModsModController> logger, INexusModsAPIClient nexusModsAPIClient, IUnitOfWorkFactory unitOfWorkFactory)
    {
        _logger = logger;
        _nexusModsAPIClient = nexusModsAPIClient;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    [HttpGet("Raws")]
    public async Task<ApiResult<RawNexusModsModModel?>> GetRawAsync([FromQuery, Required] NexusModsModId modId, [BindUserId] NexusModsUserId userId, [BindTenant] TenantId tenant, CancellationToken ct)
    {
        if (HttpContext.GetAPIKey() is { } apiKey && apiKey != NexusModsApiKey.None)
            return await GetRawWithApiKeyAsync(apiKey, modId, userId, tenant, ct);

        // TODO:
        return ApiBadRequest("Token auth not supported yet!");
    }
    private async Task<ApiResult<RawNexusModsModModel?>> GetRawWithApiKeyAsync(NexusModsApiKey apiKey, NexusModsModId modId, NexusModsUserId userId, TenantId tenant, CancellationToken ct)
    {
        var gameDomain = tenant.ToGameDomain();

        if (await _nexusModsAPIClient.GetModAsync(gameDomain, modId, apiKey, ct) is not { } modInfo)
            return ApiBadRequest("Mod not found!");

        if (userId != modInfo.User.Id)
            return ApiBadRequest("User does not have access to the mod!");

        return ApiResult(new RawNexusModsModModel(modInfo.Id, modInfo.Name));
    }


    [HttpPost("ModuleManualLinks")]
    [ButrNexusModsAuthorization(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    public async Task<ApiResult<string?>> AddModuleManualLinkAsync([FromQuery, Required] NexusModsModId modId, [FromQuery, Required] ModuleId moduleId, [BindTenant] TenantId tenant)
    {
        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite();

        var nexusModsModToModule = new NexusModsModToModuleEntity
        {
            TenantId = tenant,
            NexusModsModId = modId,
            NexusModsMod = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsMod(modId),
            ModuleId = moduleId,
            Module = unitOfWrite.UpsertEntityFactory.GetOrCreateModule(moduleId),
            LinkType = NexusModsModToModuleLinkType.ByStaff,
            LastUpdateDate = DateTimeOffset.UtcNow,
        };
        unitOfWrite.NexusModsModModules.Upsert(nexusModsModToModule);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return ApiResult("Linked successful!");
    }

    [HttpDelete("ModuleManualLinks")]
    [ButrNexusModsAuthorization(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    public async Task<ApiResult<string?>> RemoveModuleManualLinkAsync([FromQuery, Required] NexusModsModId modId, [FromQuery, Required] ModuleId moduleId)
    {
        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite();

        unitOfWrite.NexusModsModModules
            .Remove(x => x.Module.ModuleId == moduleId && x.NexusModsMod.NexusModsModId == modId && x.LinkType == NexusModsModToModuleLinkType.ByStaff);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return ApiResult("Unlinked successful!");
    }

    [HttpPost("ModuleManualLinks/Paginated")]
    [ButrNexusModsAuthorization(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    public async Task<ApiResult<PagingData<LinkedByStaffModuleNexusModsModsModel>?>> GetModuleManualLinkPaginatedAsync([FromBody, Required] PaginatedQuery query, CancellationToken ct)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var paginated = await unitOfRead.NexusModsModModules.GetByStaffPaginatedAsync(query, ct);

        return ApiPagingResult(paginated);
    }
}