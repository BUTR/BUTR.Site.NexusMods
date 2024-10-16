using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Repositories;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), ButrNexusModsAuthorization, TenantRequired]
public class ExposedModsController : ApiControllerBase
{
    private readonly ILogger _logger;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public ExposedModsController(ILogger<ExposedModsController> logger, IUnitOfWorkFactory unitOfWorkFactory)
    {
        _logger = logger;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    [HttpPost("NexusModsMod/Paginated")]
    public async Task<ApiResult<PagingData<LinkedByExposureNexusModsModModelsModel>?>> GetNexusModsModPaginatedAsync([FromBody, Required] PaginatedQuery query, CancellationToken ct)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var paginated = await unitOfRead.NexusModsModModules.GetExposedPaginatedAsync(query, ct);

        return ApiPagingResult(paginated);
    }

    [HttpGet("NexusModsMod/Autocomplete/ModuleIds")]
    public async Task<ApiResult<IList<string>?>> GetNexusModsModAutocompleteModuleIdsAsync([FromQuery, Required] ModuleId moduleId)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var moduleIds = await unitOfRead.Autocompletes.AutocompleteStartsWithAsync<NexusModsModToModuleEntity, ModuleId>(x => x.ModuleId, moduleId, CancellationToken.None);

        return ApiResult(moduleIds);
    }

    [HttpPost("SteamWorkshopMod/Paginated")]
    public async Task<ApiResult<PagingData<LinkedByExposureSteamWorkshopModModelsModel>?>> GetSteamWorkshopModPaginatedAsync([FromBody, Required] PaginatedQuery query, CancellationToken ct)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var paginated = await unitOfRead.SteamWorkshopModModules.GetExposedPaginatedAsync(query, ct);

        return ApiPagingResult(paginated);
    }

    [HttpGet("SteamWorkshopMod/Autocomplete/ModuleIds")]
    public async Task<ApiResult<IList<string>?>> GetSteamWorkshopModAutocompleteModuleIdsAsync([FromQuery, Required] ModuleId moduleId)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var moduleIds = await unitOfRead.Autocompletes.AutocompleteStartsWithAsync<SteamWorkshopModToModuleEntity, ModuleId>(x => x.ModuleId, moduleId, CancellationToken.None);

        return ApiResult(moduleIds);
    }
}