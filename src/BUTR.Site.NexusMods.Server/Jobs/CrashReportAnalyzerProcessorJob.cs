using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Jobs
{
    [DisallowConcurrentExecution]
    public sealed class CrashReportAnalyzerProcessorJob : IJob
    {
        private readonly ILogger _logger;
        private readonly AppDbContext _dbContext;

        public CrashReportAnalyzerProcessorJob(ILogger<CrashReportAnalyzerProcessorJob> logger, AppDbContext dbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var crashReportEntity = _dbContext.Model.FindEntityType(typeof(CrashReportEntity))!;
            var crashReportEntityTable = crashReportEntity.GetSchemaQualifiedTableName();
            var modIds = crashReportEntity.GetProperty(nameof(CrashReportEntity.ModIds)).GetColumnName();
            var involvedModIds = crashReportEntity.GetProperty(nameof(CrashReportEntity.InvolvedModIds)).GetColumnName();
            var gameVersion = crashReportEntity.GetProperty(nameof(CrashReportEntity.GameVersion)).GetColumnName();
            var modIdToVersion = crashReportEntity.GetProperty(nameof(CrashReportEntity.ModIdToVersion)).GetColumnName();
            
            var crashScoreInvolvedEntity = _dbContext.Model.FindEntityType(typeof(CrashScoreInvolvedEntity))!;
            var crashScoreInvolvedEntityTable = crashScoreInvolvedEntity.GetSchemaQualifiedTableName();
            
            if (!_dbContext.Database.IsNpgsql()) return;
            
            var sql = $"""
TRUNCATE TABLE {crashScoreInvolvedEntityTable};
WITH
-- Get a list of all distinct mod versions in the database
all_mod_versions AS (
    SELECT DISTINCT (EACH({modIdToVersion})).key AS mod_id, (EACH({modIdToVersion})).value AS version
    FROM {crashReportEntityTable}
),
-- Calculate the number of times a mod is involved in the crash for each mod version
mod_counts AS (
    SELECT {gameVersion}, mod_id, {modIdToVersion} -> mod_id AS version, count(*) AS count
    FROM (
             SELECT UNNEST({modIds}) AS mod_id, {modIdToVersion}, {gameVersion}
             FROM {crashReportEntityTable}
         ) t
    GROUP BY {gameVersion}, mod_id, version
),
-- Calculate the number of times where the mod version was involved in the crash
involved_mod_counts AS (
    SELECT {gameVersion}, mod_id, {modIdToVersion} -> mod_id AS version, count(*) AS count
    FROM (
             SELECT UNNEST({involvedModIds}) AS mod_id, {modIdToVersion}, {gameVersion}, {involvedModIds}
             FROM {crashReportEntityTable}
         ) t
    WHERE {involvedModIds} @> ARRAY[mod_id]
    GROUP BY {gameVersion}, mod_id, version
),
-- Calculate the number of times where the mod version was not involved in the crash
not_involved_mod_counts AS (
    SELECT {gameVersion}, mod_id, {modIdToVersion} -> mod_id AS version, count(*) AS count
    FROM (
             SELECT UNNEST({modIds}) AS mod_id, {modIdToVersion}, {gameVersion}, {involvedModIds}
             FROM {crashReportEntityTable}
         ) t
    WHERE NOT {involvedModIds} @> ARRAY[mod_id]
    GROUP BY {gameVersion}, mod_id, version 
)
INSERT INTO {crashScoreInvolvedEntityTable}
-- Calculate the score for each mod version
SELECT
    mod_counts.{gameVersion},
    all_mod_versions.mod_id,
    all_mod_versions.version,
    COALESCE(involved_mod_counts.count, 0) AS involved_count,
    COALESCE(not_involved_mod_counts.count, 0) AS not_involved_count,
    mod_counts.count AS total_count,
    involved_mod_counts.count AS value,
    COALESCE(involved_mod_counts.count, 0) / mod_counts.count::numeric AS crash_score
FROM
    all_mod_versions
        JOIN mod_counts ON
                all_mod_versions.mod_id = mod_counts.mod_id AND
                all_mod_versions.version = mod_counts.version
        JOIN involved_mod_counts ON
                all_mod_versions.mod_id = involved_mod_counts.mod_id AND
                all_mod_versions.version = involved_mod_counts.version AND
                mod_counts.{gameVersion} = involved_mod_counts.{gameVersion}
        JOIN not_involved_mod_counts ON
                all_mod_versions.mod_id = not_involved_mod_counts.mod_id AND
                all_mod_versions.version = not_involved_mod_counts.version AND
                mod_counts.{gameVersion} = not_involved_mod_counts.{gameVersion}
ORDER BY
    mod_counts.{gameVersion},
    all_mod_versions.mod_id,
    all_mod_versions.version;
""";
            await _dbContext.Database.ExecuteSqlRawAsync(sql);
        }
    }
}