using BUTR.Authentication.NexusMods.Authentication;
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

    public record ModVersionScore(string Version, double Score, double Value, int CountStable, int CountUnstable)
    {
        public double Count => CountStable + CountUnstable;
    }
    public record ModStorage(string Id, ModVersionScore[] Versions)
    {
        public double MeanScore => Versions.Length == 0 ? 0 : 1 - (Versions.Sum(x => x.Value) / (double) Versions.Sum(x => x.Count));
    };

    public record GameStorage(string GameVersion, ModStorage[] Mods);

    private readonly ILogger _logger;
    private readonly AppDbContext _dbContext;

    public StatisticsController(ILogger<StatisticsController> logger, AppDbContext dbContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    [HttpGet("AutocompleteGameVersion")]
    [Produces("application/json")]
    public ActionResult<APIResponse<IQueryable<string>?>> AutocompleteGameVersion([FromQuery] string gameVersion)
    {
        return APIResponse(_dbContext.AutocompleteStartsWith<CrashReportEntity, string>(x => x.GameVersion, gameVersion));
    }

    [HttpGet("AutocompleteModId")]
    [Produces("application/json")]
    public ActionResult<APIResponse<IQueryable<string>?>> AutocompleteModId([FromQuery] string modId)
    {
        return APIResponse(_dbContext.AutocompleteStartsWith<CrashReportEntity, string[]>(x => x.ModIds, modId));
    }

    [HttpGet("TopExceptionsTypes")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<IEnumerable<TopExceptionsEntry>?>>> TopExceptionsTypes(CancellationToken ct)
    {
        var types = await _dbContext.Set<StatisticsTopExceptionsTypeEntity>().ToArrayAsync(ct);
        var total = (decimal) types.Sum(x => x.Count);
        return APIResponse(types.Select(x => new TopExceptionsEntry(x.Type, ((decimal) x.Count / total) * 100M)));
    }

    [HttpGet("Involved")]
    [Produces("application/json")]
    public ActionResult<APIResponse<IEnumerable<GameStorage>?>> Involved([FromQuery] string[]? gameVersions, [FromQuery] string[]? modIds, [FromQuery] string[]? modVersions)
    {
        //if (gameVersions?.Length == 0 && modIds?.Length == 0 && modVersions?.Length == 0)
        //    return StatusCode(StatusCodes.Status403Forbidden, Array.Empty<GameStorage>());

        var data = _dbContext.Set<StatisticsCrashScoreInvolvedEntity>()
            .Where(x =>
                (gameVersions == null || gameVersions.Length == 0 || gameVersions.Contains(x.GameVersion)) &&
                (modIds == null || modIds.Length == 0 || modIds.Contains(x.ModId)) &&
                (modVersions == null || modVersions.Length == 0 || modVersions.Contains(x.ModVersion)))
            .AsEnumerable()
            .GroupBy(x => x.GameVersion, x => x)
            .Select(x => new GameStorage(x.Key, x
                .GroupBy(y => y.ModId)
                .Select(y => new ModStorage(y.Key, y
                    .GroupBy(z => z.ModVersion)
                    .SelectMany(z => z)
                    .Select(z => new ModVersionScore(z.ModVersion, 1 - z.Score, z.RawValue, z.NotInvolvedCount, z.InvolvedCount)).ToArray())).ToArray()));

        return APIResponse(data);
    }
}