using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.APIResponses;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), ButrNexusModsAuthorization, TenantRequired]
public sealed class StatisticsController : ControllerExtended
{
    public record TopExceptionsEntry(ExceptionTypeId Type, decimal Percentage);

    public record VersionScore
    {
        public required ModuleVersion Version { get; init; }
        public required double Score { get; init; }
        public required double Value { get; init; }
        public required int CountStable { get; init; }
        public required int CountUnstable { get; init; }
        public double Count => CountStable + CountUnstable;
    }
    public record VersionStorage
    {
        public required ModuleVersion Version { get; init; }
        public required VersionScore[] Scores { get; init; }
        public double MeanScore => Scores.Length == 0 ? 0 : 1 - (Scores.Sum(x => x.Value) / (double) Scores.Sum(x => x.Count));
    };
    public record ModuleStorage
    {
        public required ModuleId ModuleId { get; init; }
        public required VersionStorage[] Versions { get; init; }
    };

    public record GameStorage
    {
        public required GameVersion GameVersion { get; init; }
        public required ModuleStorage[] Modules { get; init; }
    }

    private readonly ILogger _logger;
    private readonly IAppDbContextRead _dbContextRead;

    public StatisticsController(ILogger<StatisticsController> logger, IAppDbContextRead dbContextRead)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContextRead = dbContextRead ?? throw new ArgumentNullException(nameof(dbContextRead));
    }

    [HttpGet("AutocompleteGameVersion")]
    [Produces("application/json")]
    public APIResponseActionResult<IQueryable<string>?> AutocompleteGameVersion([FromQuery] GameVersion gameVersion)
    {
        return APIResponse(_dbContextRead.AutocompleteStartsWith<CrashReportEntity, GameVersion>(x => x.GameVersion, gameVersion));
    }

    [HttpGet("AutocompleteModuleId")]
    [Produces("application/json")]
    public APIResponseActionResult<IQueryable<string>?> AutocompleteModuleId([FromQuery] ModuleId moduleId)
    {
        return APIResponse(_dbContextRead.AutocompleteStartsWith<CrashReportToModuleMetadataEntity, ModuleId>(x => x.Module.ModuleId, moduleId));
    }

    [HttpGet("TopExceptionsTypes")]
    [Produces("application/json")]
    public async Task<APIResponseActionResult<IEnumerable<TopExceptionsEntry>?>> TopExceptionsTypesAsync(CancellationToken ct)
    {
        var types = await _dbContextRead.StatisticsTopExceptionsTypes
            .Include(x => x.ExceptionType)
            .ToArrayAsync(ct);

        var total = (decimal) types.Sum(x => x.ExceptionCount);

        return APIResponse(types.Select(x => new TopExceptionsEntry(x.ExceptionType.ExceptionTypeId, ((decimal) x.ExceptionCount / total) * 100M)));
    }

    [HttpGet("Involved")]
    [Produces("application/json")]
    public APIResponseActionResult<IQueryable<GameStorage>?> Involved([FromQuery] GameVersion[]? gameVersions, [FromQuery] ModuleId[]? moduleIds, [FromQuery] ModuleVersion[]? moduleVersions)
    {
        //if (gameVersions?.Length == 0 && modIds?.Length == 0 && modVersions?.Length == 0)
        //    return StatusCode(StatusCodes.Status403Forbidden, Array.Empty<GameStorage>());

        var data = _dbContextRead.StatisticsCrashScoreInvolveds
            .Include(x => x.Module)
            .WhereIf(gameVersions != null && gameVersions.Length != 0, x => gameVersions!.Contains(x.GameVersion))
            .WhereIf(moduleIds != null && moduleIds.Length != 0, x => moduleIds!.Contains(x.Module.ModuleId))
            .WhereIf(moduleVersions != null && moduleVersions.Length != 0, x => moduleVersions!.Contains(x.ModuleVersion))
            .GroupBy(x => new { x.GameVersion })
            .Select(x => new GameStorage
            {
                GameVersion = x.Key.GameVersion,
                Modules = x.GroupBy(y => new { y.Module.ModuleId }).Select(y => new ModuleStorage
                {
                    ModuleId = y.Key.ModuleId,
                    Versions = y.GroupBy(z => new { z.ModuleVersion }).Select(z => new VersionStorage
                    {
                        Version = z.Key.ModuleVersion,
                        Scores = z.Select(q => new VersionScore
                        {
                            Version = z.Key.ModuleVersion,
                            Score = 1 - q.Score,
                            Value = q.RawValue,
                            CountStable = q.NotInvolvedCount,
                            CountUnstable = q.InvolvedCount,
                        }).ToArray(),
                    }).ToArray(),
                }).ToArray(),
            });

        return APIResponse(data);
    }
}