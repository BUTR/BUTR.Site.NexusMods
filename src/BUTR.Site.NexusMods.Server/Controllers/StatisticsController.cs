﻿using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
public sealed class StatisticsController : ControllerExtended
{
    public record TopExceptionsEntry(string Type, decimal Percentage);

    public record VersionScore
    {
        public required string Version { get; init; }
        public required double Score { get; init; }
        public required double Value { get; init; }
        public required int CountStable { get; init; }
        public required int CountUnstable { get; init; }
        public double Count => CountStable + CountUnstable;
    }
    public record VersionStorage
    {
        public required string Version { get; init; }
        public required VersionScore[] Scores { get; init; }
        public double MeanScore => Scores.Length == 0 ? 0 : 1 - (Scores.Sum(x => x.Value) / (double) Scores.Sum(x => x.Count));
    };
    public record ModuleStorage
    {
        public required string ModuleId { get; init; }
        public required VersionStorage[] Versions { get; init; }
    };

    public record GameStorage
    {
        public required string GameVersion { get; init; }
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
    public ActionResult<APIResponse<IQueryable<string>?>> AutocompleteGameVersion([FromQuery] string gameVersion)
    {
        return APIResponse(_dbContextRead.AutocompleteStartsWith<CrashReportEntity>(x => x.GameVersion, gameVersion));
    }

    [HttpGet("AutocompleteModuleId")]
    [Produces("application/json")]
    public ActionResult<APIResponse<IQueryable<string>?>> AutocompleteModuleId([FromQuery] string moduleId)
    {
        return APIResponse(_dbContextRead.AutocompleteStartsWith<CrashReportToModuleMetadataEntity>(x => x.Module.ModuleId, moduleId));
    }

    [HttpGet("TopExceptionsTypes")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<IEnumerable<TopExceptionsEntry>?>>> TopExceptionsTypesAsync(CancellationToken ct)
    {
        var types = await _dbContextRead.StatisticsTopExceptionsTypes
            .Include(x => x.ExceptionType)
            .ToArrayAsync(ct);

        var total = (decimal) types.Sum(x => x.ExceptionCount);

        return APIResponse(types.Select(x => new TopExceptionsEntry(x.ExceptionType.ExceptionTypeId, ((decimal) x.ExceptionCount / total) * 100M)));
    }

    [HttpGet("Involved")]
    [Produces("application/json")]
    public ActionResult<APIResponse<IQueryable<GameStorage>?>> Involved([FromQuery] string[]? gameVersions, [FromQuery] string[]? moduleIds, [FromQuery] string[]? moduleVersions)
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
                            CountUnstable = q.InvolvedCount
                        }).ToArray()
                    }).ToArray()
                }).ToArray()
            });

        return APIResponse(data);
    }
}