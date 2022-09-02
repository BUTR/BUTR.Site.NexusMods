using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Options;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Npgsql;

using Quartz;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Jobs
{
    [DisallowConcurrentExecution]
    public sealed class CrashReportProcessorJob : IJob
    {
        private readonly ILogger _logger;
        private readonly CrashReporterOptions _options;
        private readonly CrashReporterClient _client;
        private readonly AppDbContext _dbContext;

        public CrashReportProcessorJob(ILogger<CrashReportProcessorJob> logger, IOptions<CrashReporterOptions> options, CrashReporterClient client, AppDbContext dbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await foreach (var (report, date) in MissingFilenames(_dbContext, context.CancellationToken))
            {
                var crashReportEntity = new CrashReportEntity
                {
                    Id = report.Id,
                    Url = new Uri(new Uri(_options.Endpoint), $"{report.Id2}.html").ToString(),
                    GameVersion = report.GameVersion,
                    Exception = report.Exception,
                    CreatedAt = report.Id2.Length == 8 ? DateTime.UnixEpoch : date,
                    ModIds = report.Modules.Select(x => x.Id).ToImmutableArray().AsArray(),
                    InvolvedModIds = report.InvolvedModules.Select(x => x.Id).ToImmutableArray().AsArray(),
                    ModNexusModsIds = report.Modules
                        .Select(x => ModsUtils.TryParse(x.Url, out _, out var modId) ? modId.GetValueOrDefault(-1) : -1)
                        .Where(x => x != -1)
                        .ToImmutableArray().AsArray(),
                };

                CrashReportEntity? ApplyChangesCrashReportEntity(CrashReportEntity? existing) => existing switch
                {
                    _ => crashReportEntity,
                };
                await _dbContext.AddUpdateRemoveAndSaveAsync<CrashReportEntity>(x => x.Id == report.Id, ApplyChangesCrashReportEntity, context.CancellationToken);

                CrashReportFileEntity? ApplyChangesCrashReportFileEntity(CrashReportFileEntity? existing) => existing switch
                {
                    _ => new CrashReportFileEntity { Filename = report.Id2, CrashReport = crashReportEntity },
                };
                await _dbContext.AddUpdateRemoveAndSaveAsync<CrashReportFileEntity>(x => x.Filename == report.Id2, ApplyChangesCrashReportFileEntity, context.CancellationToken);
            }
        }

        private async IAsyncEnumerable<(CrashRecord, DateTime)> MissingFilenames(AppDbContext dbContext, [EnumeratorCancellation] CancellationToken ct)
        {
            var mapping = dbContext.Model.FindEntityType(typeof(CrashReportFileEntity));
            if (mapping is null || mapping.GetTableName() is not { } table)
                yield break;

            var schema = mapping.GetSchema();
            var tableName = mapping.GetSchemaQualifiedTableName();
            var storeObjectIdentifier = StoreObjectIdentifier.Table(table, schema);
            var filenameName = mapping.GetProperty(nameof(CrashReportFileEntity.Filename)).GetColumnName(storeObjectIdentifier);

            var filenames = await _client.GetCrashReportNamesAsync(ct);

            var ignored = dbContext.Set<CrashReportIgnoredFilesEntity>().AsNoTracking().Select(x => x.Filename).ToHashSet();
            filenames.ExceptWith(ignored);

            for (var skip = 0; skip < filenames.Count; skip += 1000)
            {
                var take = filenames.Count - skip >= 1000 ? 1000 : filenames.Count - skip;
                var toFindFilenames = new NpgsqlParameter<List<string>>("filenames", filenames.Skip(skip).Take(take).ToList());
                var query = dbContext.Set<CrashReportFileEntity>()
                    .FromSqlRaw(@$"
SELECT
    {filenameName}
FROM
    unnest(@filenames) as {filenameName}
WHERE
    {filenameName} NOT IN (SELECT {filenameName} FROM {tableName})", toFindFilenames)
                    .AsNoTracking()
                    .Select(x => x.Filename);

                var missingFilenames = await query.ToImmutableArrayAsync(ct);
                foreach (var missing in await _client.GetCrashReportDatesAsync(missingFilenames, ct))
                {
                    var reportStr = await _client.GetCrashReportAsync(missing.Filename, ct);
                    var report = CrashReportParser.Parse(missing.Filename, reportStr);
                    var foundExisting = await dbContext.Set<CrashReportEntity>().Where(x => x.Id == report.Id).Select(x => x.Id).CountAsync(ct) > 0;
                    if (string.IsNullOrEmpty(reportStr) || string.IsNullOrEmpty(report.GameVersion) || foundExisting)
                    {
                        CrashReportIgnoredFilesEntity? ApplyChanges(CrashReportIgnoredFilesEntity? existing) => existing switch
                        {
                            null => new() { Filename = missing.Filename, Id = report.Id },
                            var entity => entity,
                        };
                        await dbContext.AddUpdateRemoveAndSaveAsync<CrashReportIgnoredFilesEntity>(x => x.Filename == missing.Filename, ApplyChanges, ct);
                        continue;
                    }

                    yield return (report, missing.Date);
                }
            }
        }
    }
}