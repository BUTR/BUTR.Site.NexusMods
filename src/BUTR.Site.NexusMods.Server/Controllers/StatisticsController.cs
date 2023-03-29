using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

using Npgsql;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<ActionResult> AutocompleteGameVersion([FromQuery] string gameVersion)
        {
            var mapping = _dbContext.Model.FindEntityType(typeof(CrashReportEntity));
            if (mapping is null || mapping.GetTableName() is not { } table)
                return StatusCode(StatusCodes.Status200OK, Array.Empty<string>());

            var schema = mapping.GetSchema();
            var tableName = mapping.GetSchemaQualifiedTableName();
            var storeObjectIdentifier = StoreObjectIdentifier.Table(table, schema);
            var gameVersionName = mapping.GetProperty(nameof(CrashReportEntity.GameVersion)).GetColumnName(storeObjectIdentifier);
            var modIdsName = mapping.GetProperty(nameof(CrashReportEntity.ModIds)).GetColumnName(storeObjectIdentifier);

            var valPram = new NpgsqlParameter<string>("val", gameVersion);
            var values = await _dbContext.Set<CrashReportEntity>()
                .FromSqlRaw(@$"
WITH values AS (SELECT DISTINCT {gameVersionName} as game_versions FROM {tableName} ORDER BY game_versions)
SELECT
    array_agg(game_versions) as {modIdsName}
FROM
    values
WHERE
    game_versions ILIKE @val || '%'", valPram)
                .AsNoTracking()
                .Select(x => x.ModIds)
                .ToArrayAsync();

            return StatusCode(StatusCodes.Status200OK, values.First());
        }

        [HttpGet("AutocompleteModId")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> AutocompleteModId([FromQuery] string modId)
        {
            var mapping = _dbContext.Model.FindEntityType(typeof(CrashReportEntity));
            if (mapping is null || mapping.GetTableName() is not { } table)
                return StatusCode(StatusCodes.Status200OK, Array.Empty<string>());

            var schema = mapping.GetSchema();
            var tableName = mapping.GetSchemaQualifiedTableName();
            var storeObjectIdentifier = StoreObjectIdentifier.Table(table, schema);
            var modIdsName = mapping.GetProperty(nameof(CrashReportEntity.ModIds)).GetColumnName(storeObjectIdentifier);

            var valPram = new NpgsqlParameter<string>("val", modId);
            var values = await _dbContext.Set<CrashReportEntity>()
                .FromSqlRaw(@$"
WITH values AS (SELECT DISTINCT unnest({modIdsName}) as {modIdsName} FROM {tableName} ORDER BY {modIdsName})
SELECT
    array_agg({modIdsName}) as {modIdsName}
FROM
    values
WHERE
    {modIdsName} ILIKE @val || '%'", valPram)
                .AsNoTracking()
                .Select(x => x.ModIds)
                .ToArrayAsync();

            return StatusCode(StatusCodes.Status200OK, values.First());
        }

        [HttpGet("AutocompleteModVersion")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> AutocompleteModId([FromQuery] string modId, string modVersion)
        {
            var mapping = _dbContext.Model.FindEntityType(typeof(CrashReportEntity));
            if (mapping is null || mapping.GetTableName() is not { } table)
                return StatusCode(StatusCodes.Status200OK, Array.Empty<string>());

            var schema = mapping.GetSchema();
            var tableName = mapping.GetSchemaQualifiedTableName();
            var storeObjectIdentifier = StoreObjectIdentifier.Table(table, schema);
            var modIdsName = mapping.GetProperty(nameof(CrashReportEntity.ModIds)).GetColumnName(storeObjectIdentifier);
            var modIdToVersionName = mapping.GetProperty(nameof(CrashReportEntity.ModIdToVersion)).GetColumnName(storeObjectIdentifier);

            var modIdPram = new NpgsqlParameter<string>("modId", modId);
            var valPram = new NpgsqlParameter<string>("val", modVersion);
            var values = await _dbContext.Set<CrashReportEntity>()
                .FromSqlRaw(@$"
WITH values AS (SELECT DISTINCT {modIdToVersionName} -> @modId as mod_version FROM {tableName} WHERE exist({modIdToVersionName}, @modId) ORDER BY mod_version)
SELECT
    array_agg(mod_version) as {modIdsName}
FROM
    values
WHERE
    mod_version ILIKE @val || '%'", modIdPram, valPram)
                .AsNoTracking()
                .Select(x => x.GameVersion)
                .ToArrayAsync();

            return StatusCode(StatusCodes.Status200OK, values.First());
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