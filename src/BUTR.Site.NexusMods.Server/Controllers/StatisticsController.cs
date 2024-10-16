using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Repositories;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), ButrNexusModsAuthorization, TenantRequired]
public sealed class StatisticsController : ApiControllerBase
{
    public record TopExceptionsEntry(ExceptionTypeId Type, decimal Percentage);

    private readonly ILogger _logger;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public StatisticsController(ILogger<StatisticsController> logger, IUnitOfWorkFactory unitOfWorkFactory)
    {
        _logger = logger;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    [HttpGet("TopExceptionsTypes")]
    public async Task<ApiResult<IEnumerable<TopExceptionsEntry>?>> GetTopExceptionsTypesAsync(CancellationToken ct)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var types = await unitOfRead.StatisticsTopExceptionsTypes.GetAllAsync(null, null, ct);

        var total = (decimal) types.Sum(x => x.ExceptionCount);

        return ApiResult(types.Select(x => new TopExceptionsEntry(x.ExceptionType.ExceptionTypeId, ((decimal) x.ExceptionCount / total) * 100M)));
    }

    [HttpGet("InvolvedModules")]
    public async Task<ApiResult<IList<StatisticsInvolvedModuleScoresForGameVersionModel>?>> GetInvolvedModulesAsync([FromQuery] GameVersion[]? gameVersions, [FromQuery] ModuleId[]? moduleIds, [FromQuery] ModuleVersion[]? moduleVersions)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        //if (gameVersions?.Length == 0 && modIds?.Length == 0 && modVersions?.Length == 0)
        //    return StatusCode(StatusCodes.Status403Forbidden, Array.Empty<GameStorage>());

        var data = await unitOfRead.StatisticsCrashScoreInvolveds.GetAllInvolvedModuleScoresForGameVersionAsync(gameVersions, moduleIds, moduleVersions, CancellationToken.None);

        return ApiResult(data);
    }

    [HttpGet("CrashReportsPerDay")]
    public async Task<ApiResult<IList<StatisticsCrashReportsPerDateModel>?>> GetCrashReportsPerDayAsync(
        [FromQuery] DateOnly from, [FromQuery] DateOnly to,
        [FromQuery] NexusModsModId[]? modIds, [FromQuery] GameVersion[]? gameVersions, [FromQuery] ModuleId[]? moduleIds, [FromQuery] ModuleVersion[]? moduleVersions)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var data = await unitOfRead.StatisticsCrashReportsPerDay.GetAllAsync(from, to, modIds, gameVersions, moduleIds, moduleVersions, CancellationToken.None);

        return ApiResult(data);
    }

    [HttpGet("CrashReportsPerMonth")]
    public async Task<ApiResult<IList<StatisticsCrashReportsPerDateModel>?>> GetCrashReportsPerMonthAsync(
        [FromQuery] DateOnly from, [FromQuery] DateOnly to,
        [FromQuery] NexusModsModId[]? modIds, [FromQuery] GameVersion[]? gameVersions, [FromQuery] ModuleId[]? moduleIds, [FromQuery] ModuleVersion[]? moduleVersions)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var data = await unitOfRead.StatisticsCrashReportsPerMonth.GetAllAsync(from, to, modIds, gameVersions, moduleIds, moduleVersions, CancellationToken.None);

        return ApiResult(data);
    }


    [HttpGet("Autocomplete/GameVersions")]
    public async Task<ApiResult<IList<string>?>> GetAutocompleteGameVersionsAsync([FromQuery, Required] GameVersion gameVersion)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var gameVersions = await unitOfRead.Autocompletes.AutocompleteStartsWithAsync<CrashReportEntity, GameVersion>(x => x.GameVersion, gameVersion, CancellationToken.None);

        return ApiResult(gameVersions);
    }

    [HttpGet("Autocomplete/ModuleIds")]
    public async Task<ApiResult<IList<string>?>> GetAutocompleteModuleIdsAsync([FromQuery, Required] ModuleId moduleId)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var moduleIds = await unitOfRead.Autocompletes.AutocompleteStartsWithAsync<CrashReportToModuleMetadataEntity, ModuleId>(x => x.ModuleId, moduleId, CancellationToken.None);

        return ApiResult(moduleIds);
    }
}