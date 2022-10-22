using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Options;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Quartz;

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Jobs
{
    [DisallowConcurrentExecution]
    public sealed class CrashReportVersionProcessorJob : IJob
    {
        private readonly ILogger _logger;
        private readonly HttpClient _client;
        private readonly AppDbContext _dbContext;

        public CrashReportVersionProcessorJob(ILogger<CrashReportVersionProcessorJob> logger, HttpClient client, AppDbContext dbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var ct = context.CancellationToken;
            var query = _dbContext.Set<CrashReportEntity>().Where(x => x.Version == 0).Take(500).AsNoTracking();
            while (await query.ToArrayAsync(ct) is { Length: > 0 } crashReports)
            {
                foreach (var crashReport in crashReports)
                {
                    var reportStr = await _client.GetStringAsync(crashReport.Url, ct);
                    var report = CrashReportParser.Parse(string.Empty, reportStr);

                    CrashReportEntity? ApplyChanges(CrashReportEntity? existing) => existing switch
                    {
                        null => null,
                        var entity => entity with { Version = report.Version },
                    };
                    await _dbContext.AddUpdateRemoveAndSaveAsync<CrashReportEntity>(x => x.Id == crashReport.Id, ApplyChanges, ct);
                }
            }
        }
    }
}