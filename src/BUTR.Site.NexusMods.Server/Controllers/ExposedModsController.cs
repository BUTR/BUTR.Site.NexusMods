using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), ButrNexusModsAuthorization, TenantRequired]
public class ExposedModsController : ApiControllerBase
{
    public record ExposedNexusModsModModel(NexusModsModId NexusModsModId, ExposedModuleModel[] Mods);
    public record ExposedModuleModel(ModuleId ModuleId, DateTimeOffset LastCheckedDate);


    private readonly ILogger _logger;
    private readonly IAppDbContextRead _dbContextRead;

    public ExposedModsController(ILogger<ExposedModsController> logger, IAppDbContextRead dbContextRead)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContextRead = dbContextRead ?? throw new ArgumentNullException(nameof(dbContextRead));
    }

    [HttpPost("Paginated")]
    public async Task<ApiResult<PagingData<ExposedNexusModsModModel>?>> PaginatedAsync([FromBody] PaginatedQuery query, CancellationToken ct)
    {
        var paginated = await _dbContextRead.NexusModsModModules
            .Where(x => x.LinkType == NexusModsModToModuleLinkType.ByUnverifiedFileExposure)
            .PaginatedAsync(query, 100, new() { Property = nameof(NexusModsModEntity.NexusModsModId), Type = SortingType.Ascending }, ct);

        return ApiPagingResult(paginated, items => items.GroupBy(x => x.NexusModsMod.NexusModsModId).SelectAwaitWithCancellation(async (x, ct2) =>
            new ExposedNexusModsModModel(x.Key, await x.Select(y => new ExposedModuleModel(y.Module.ModuleId, y.LastUpdateDate.ToUniversalTime())).ToArrayAsync(ct2))));
    }

    [HttpGet("Autocomplete")]
    public ApiResult<IQueryable<string>?> Autocomplete([FromQuery] ModuleId modId)
    {
        return ApiResult(_dbContextRead.AutocompleteStartsWith<NexusModsModToModuleEntity, ModuleId>(x => x.Module.ModuleId, modId));
    }
}