using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;

namespace BUTR.Site.NexusMods.Server.Controllers
{
    [ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
    public sealed class StatisticsController : ControllerBase
    {
        private record TopExceptionsEntry(string Type, int Count);

        private record ModVersionScore(string Version, double Score, double Value, int CountStable, int CountUnstable)
        {
            public double Count => CountStable + CountUnstable;
        }
        private record ModStorage(string Id, ModVersionScore[] Versions)
        {
            public double MeanScore => Versions.Length == 0 ? 0 : 1 - (Versions.Sum(x => x.Value) / (double) Versions.Sum(x => x.Count));
        };
        private record GameStorage(string GameVersion, ModStorage[] Mods);

        private readonly ILogger _logger;
        private readonly AppDbContext _dbContext;

        public StatisticsController(ILogger<StatisticsController> logger, AppDbContext dbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        [HttpGet("AutocompleteGameVersion")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public ActionResult AutocompleteGameVersion([FromQuery] string gameVersion)
        {
            return StatusCode(StatusCodes.Status200OK, _dbContext.AutocompleteStartsWith<CrashReportEntity, string>(x => x.GameVersion, gameVersion));
        }

        [HttpGet("AutocompleteModId")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public ActionResult AutocompleteModId([FromQuery] string modId)
        {
            return StatusCode(StatusCodes.Status200OK, _dbContext.AutocompleteStartsWith<CrashReportEntity, string[]>(x => x.ModIds, modId));
        }

        [HttpGet("TopExceptionsTypes")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<TopExceptionsEntry>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public ActionResult TopExceptionsTypes()
        {
            return StatusCode(StatusCodes.Status200OK, _dbContext.Set<TopExceptionsTypeEntity>());
        }

        [HttpGet("Involved")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<GameStorage>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public ActionResult Involved([FromQuery] string[]? gameVersions, [FromQuery] string[]? modIds, [FromQuery] string[]? modVersions)
        {
            //if (gameVersions?.Length == 0 && modIds?.Length == 0 && modVersions?.Length == 0)
            //    return StatusCode(StatusCodes.Status403Forbidden, Array.Empty<GameStorage>());

            var data = _dbContext.Set<CrashScoreInvolvedEntity>()
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

            return StatusCode(StatusCodes.Status200OK, data);
        }
    }
}