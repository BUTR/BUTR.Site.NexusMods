using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Options;
using BUTR.Site.NexusMods.Server.Services.Database;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        private readonly CrashReportFileProvider _fileProvider;
        private readonly CrashReportsProvider _provider;

        public CrashReportHandler(
            ILogger<CrashReportHandler> logger, IOptions<CrashReporterOptions> options,
            CrashReporterClient client, CrashReportFileProvider fileProvider, CrashReportsProvider provider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var missings = await MissingFilenames(ct).ToHashSetAsync(ct);
                foreach (var (filename, date) in missings)
                {
                    var reportStr = await _client.GetCrashReportAsync(filename, ct);
                    var report = CrashReportParser.Parse(filename, reportStr);
                    var crashReport = await _provider.UpsertAsync(new CrashReportTableEntry
                    {
                        Id = report.Id,
                        Url = new Uri(new Uri(_options.Endpoint), $"{filename}.html").ToString(),
                        GameVersion = report.GameVersion,
                        Exception = report.Exception,
                        CreatedAt = filename.Length == 8 ? DateTime.UnixEpoch : date,
                        ModIds = report.Modules.Select(x => x.Id).ToImmutableArray(),
                        InvolvedModIds = report.InvolvedModules.Select(x => x.Id).ToImmutableArray(),
                        ModNexusModsIds = report.Modules
                            .Select(x => ModsUtils.TryParse(x.Url, out _, out var modId) ? modId.GetValueOrDefault(-1) : -1)
                            .Where(x => x != -1)
                            .ToImmutableArray(),
                    }, ct);
                    var crashReportFile = await _fileProvider.UpsertAsync(new CrashReportFileTableEntry
                    {
                        Filename = report.Id2,
                        CrashReportId = crashReport?.Id ?? report.Id
                    }, ct);
                    ;
                }

                await Task.Delay(TimeSpan.FromHours(1), ct);
            }
        }

        private async IAsyncEnumerable<FileNameDate> MissingFilenames([EnumeratorCancellation] CancellationToken ct)
        {
            var filenames = await _client.GetCrashReportNamesAsync(ct);
            for (var skip = 0; skip < filenames.Count; skip += 1000)
            {
                var take = filenames.Count - skip >= 1000 ? 1000 : filenames.Count - skip;
                var missingFilenames = await _fileProvider.FindMissingFilenames(filenames.Skip(skip).Take(take).ToArray(), ct).ToArrayAsync(ct);
                foreach (var missing in await _client.GetCrashReportDatesAsync(missingFilenames, ct))
                {
                    yield return missing;
                }
            }
        }
    }
}