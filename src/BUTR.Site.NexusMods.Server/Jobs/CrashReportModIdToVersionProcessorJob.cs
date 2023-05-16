using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using DbFunctionsExtensions = BUTR.Site.NexusMods.Server.Extensions.DbFunctionsExtensions;

namespace BUTR.Site.NexusMods.Server.Jobs;

/// <summary>
/// Part of migration process. Process all old reports and add the missing ModIdToVersion prop
/// </summary>
[DisallowConcurrentExecution]
public sealed class CrashReportModIdToVersionProcessorJob : IJob
{
    private readonly ILogger _logger;
    private readonly HttpClient _client;
    private readonly AppDbContext _dbContext;

    public CrashReportModIdToVersionProcessorJob(ILogger<CrashReportModIdToVersionProcessorJob> logger, HttpClient client, AppDbContext dbContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var ct = context.CancellationToken;
        var query = _dbContext.Set<CrashReportEntity>().Where(x => DbFunctionsExtensions.HasKeyValue(x.ModIdToVersion, "ignore", "=", "ignore")).Take(500);
        var processed = 0;
        try
        {
            while (!ct.IsCancellationRequested && await query.ToArrayAsync(ct) is { Length: > 0 } crashReports)
            {
                foreach (var crashReport in crashReports)
                {
                    var reportStr = await _client.GetStringAsync(crashReport.Url, ct);
                    var report = CrashReportParser.Parse(string.Empty, reportStr);

                    CrashReportEntity? ApplyChanges(CrashReportEntity? existing) => existing switch
                    {
                        null => null,
                        _ => existing with { ModIdToVersion = report.Modules.Select(item => new { item.Id, item.Version }).Distinct().ToDictionary(x => x.Id, x => x.Version) },
                    };
                    await _dbContext.AddUpdateRemoveAndSaveAsync<CrashReportEntity>(x => x.Id == crashReport.Id, ApplyChanges, ct);
                    processed++;
                }
            }
        }
        finally
        {
            context.Result = $"Processed {processed} crash report entities mod id to version migration";
            context.SetIsSuccess(true);
        }
    }
}