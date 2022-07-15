using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Options;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Npgsql;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services
{
    public sealed class CrashReportHandler : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly CrashReporterOptions _options;
        private readonly CrashReporterClient _client;
        private readonly IServiceScopeFactory _scopeFactory;

        public CrashReportHandler(ILogger<CrashReportHandler> logger, IOptions<CrashReporterOptions> options, CrashReporterClient client, IServiceScopeFactory scopeFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var missings = await MissingFilenames(dbContext, ct).ToHashSetAsync(ct);
                foreach (var (filename, date) in missings)
                {
                    var reportStr = await _client.GetCrashReportAsync(filename, ct);
                    var report = CrashReportParser.Parse(filename, reportStr);

                    var crashReportEntity = new CrashReportEntity
                    {
                        Id = report.Id,
                        Url = new Uri(new Uri(_options.Endpoint), $"{filename}.html").ToString(),
                        GameVersion = report.GameVersion,
                        Exception = report.Exception,
                        CreatedAt = filename.Length == 8 ? DateTime.UnixEpoch : date,
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
                    await dbContext.AddUpdateRemoveAndSaveAsync<CrashReportEntity>(x => x.Id == report.Id, ApplyChangesCrashReportEntity, ct);

                    CrashReportFileEntity? ApplyChangesCrashReportFileEntity(CrashReportFileEntity? existing) => existing switch
                    {
                        _ => new CrashReportFileEntity
                        {
                            Filename = report.Id2,
                            CrashReport = crashReportEntity
                        },
                    };
                    await dbContext.AddUpdateRemoveAndSaveAsync<CrashReportFileEntity>(x => x.Filename == report.Id2, ApplyChangesCrashReportFileEntity, ct);
                }

                await Task.Delay(TimeSpan.FromHours(1), ct);
            }
        }

        private async IAsyncEnumerable<FileNameDate> MissingFilenames(AppDbContext dbContext, [EnumeratorCancellation] CancellationToken ct)
        {
            var mapping = dbContext.Model.FindEntityType(typeof(CrashReportFileEntity));
            if (mapping is null || mapping.GetTableName() is not { } table)
                yield break;

            var schema = mapping.GetSchema();
            var tableName = mapping.GetSchemaQualifiedTableName();
            var storeObjectIdentifier = StoreObjectIdentifier.Table(table, schema);
            var filenameName = mapping.GetProperty(nameof(CrashReportFileEntity.Filename)).GetColumnName(storeObjectIdentifier);

            var filenames = await _client.GetCrashReportNamesAsync(ct);
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
                    yield return missing;
                }
            }
        }
    }
}