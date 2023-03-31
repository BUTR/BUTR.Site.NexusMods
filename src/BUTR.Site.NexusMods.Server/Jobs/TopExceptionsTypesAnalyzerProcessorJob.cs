using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Jobs
{
    [DisallowConcurrentExecution]
    public sealed class TopExceptionsTypesAnalyzerProcessorJob : IJob
    {
        private readonly ILogger _logger;
        private readonly AppDbContext _dbContext;

        public TopExceptionsTypesAnalyzerProcessorJob(ILogger<TopExceptionsTypesAnalyzerProcessorJob> logger, AppDbContext dbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var crashReportEntity = _dbContext.Model.FindEntityType(typeof(CrashReportEntity))!;
            var crashReportEntityTable = crashReportEntity.GetSchemaQualifiedTableName();
            var exception = crashReportEntity.GetProperty(nameof(CrashReportEntity.Exception)).GetColumnName();

            var topExceptionsTypeEntity = _dbContext.Model.FindEntityType(typeof(TopExceptionsTypeEntity))!;
            var topExceptionsTypeEntityTable = topExceptionsTypeEntity.GetSchemaQualifiedTableName();

            if (!_dbContext.Database.IsNpgsql()) return;


            var sql = $"""
TRUNCATE TABLE {topExceptionsTypeEntityTable};
INSERT INTO {topExceptionsTypeEntityTable}
SELECT split_part((regexp_split_to_array({exception}, '\r\n'))[3], ': ', 2) as type, COUNT(*) as count
FROM {crashReportEntityTable}
GROUP BY type
""";
            await _dbContext.Database.ExecuteSqlRawAsync(sql, new object[] { }, context.CancellationToken);
            context.Result = "Updated Top Exception Types Data";
            context.SetIsSuccess(true);
        }
    }
}