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

    [HttpPost("Paginated")]
    public async Task<ApiResult<PagingData<LinkedByExposureNexusModsModModelsModel>?>> GetPaginatedAsync([FromBody, Required] PaginatedQuery query, CancellationToken ct)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var paginated = await unitOfRead.NexusModsModModules.GetExposedPaginatedAsync(query, ct);

        return ApiPagingResult(paginated);
    }

    [HttpGet("Autocomplete/ModuleIds")]
    public async Task<ApiResult<IList<string>?>> GetAutocompleteModuleIdsAsync([FromQuery, Required] ModuleId moduleId)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var moduleIds = await unitOfRead.Autocompletes.AutocompleteStartsWithAsync<NexusModsModToModuleEntity, ModuleId>(x => x.Module.ModuleId, moduleId, CancellationToken.None);

        return ApiResult(moduleIds);
    }
}