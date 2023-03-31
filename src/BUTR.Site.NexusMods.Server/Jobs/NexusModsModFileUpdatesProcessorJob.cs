using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Options;
using BUTR.Site.NexusMods.Server.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Quartz;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Jobs
{
    /// <summary>
    /// Will be able to keep the database consistent as long as the service is not stopped for more than a day
    /// </summary>
    [DisallowConcurrentExecution]
    public sealed class NexusModsModFileUpdatesProcessorJob : IJob
    {
        private readonly ILogger _logger;
        private readonly NexusModsOptions _options;
        private readonly NexusModsAPIClient _client;
        private readonly NexusModsInfo _info;
        private readonly AppDbContext _dbContext;

        public NexusModsModFileUpdatesProcessorJob(ILogger<NexusModsModFileUpdatesProcessorJob> logger, IOptions<NexusModsOptions> options, NexusModsAPIClient client, NexusModsInfo info, AppDbContext dbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _info = info ?? throw new ArgumentNullException(nameof(info));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var updatesStoredWithinDay = await _dbContext.Set<NexusModsFileUpdateEntity>().Where(x => x.LastCheckedDate > DateTime.UtcNow.AddDays(-1)).ToListAsync(context.CancellationToken);
            var updatedWithinDay = await _client.GetAllModUpdatesAsync("mountandblade2bannerlord", _options.ApiKey) ?? Array.Empty<NexusModsUpdatedModsResponse>();
            var newUpdates = updatedWithinDay.Where(x =>
            {
                var latestFileUpdateDate = DateTimeOffset.FromUnixTimeSeconds(x.LatestFileUpdateTimestamp).UtcDateTime;
                if (latestFileUpdateDate < DateTime.UtcNow.AddDays(-1)) return false;

                var found = updatesStoredWithinDay.FirstOrDefault(y => y.NexusModsModId == x.Id);
                return found is null || found.LastCheckedDate < latestFileUpdateDate;
            }).ToList();

            context.MergedJobDataMap["UpdatesStoredWithinDay"] = updatesStoredWithinDay.Count;
            context.MergedJobDataMap["UpdatedWithinDay"] = updatedWithinDay.Length;
            context.MergedJobDataMap["NewUpdates"] = newUpdates.Count;

            var processed = 0;
            try
            {
                foreach (var modUpdate in newUpdates)
                {
                    if (context.CancellationToken.IsCancellationRequested) break;
                    
                    var exposedModIds = await _info.GetModIdsAsync("mountandblade2bannerlord", modUpdate.Id, _options.ApiKey).Distinct().ToImmutableArrayAsync(context.CancellationToken);

                    NexusModsExposedModsEntity? ApplyChanges2(NexusModsExposedModsEntity? existing) => existing switch
                    {
                        null => new() { NexusModsModId = modUpdate.Id, ModIds = exposedModIds.AsArray(), LastCheckedDate = DateTime.UtcNow },
                        _ => existing with { ModIds = existing.ModIds.AsImmutableArray().AddRange(exposedModIds.Except(existing.ModIds)).AsArray(), LastCheckedDate = DateTime.UtcNow }
                    };
                    await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsExposedModsEntity>(x => x.NexusModsModId == modUpdate.Id, ApplyChanges2, context.CancellationToken);

                    NexusModsFileUpdateEntity? ApplyChanges(NexusModsFileUpdateEntity? existing) => existing switch
                    {
                        null => new() { NexusModsModId = modUpdate.Id, LastCheckedDate = DateTimeOffset.FromUnixTimeSeconds(modUpdate.LatestFileUpdateTimestamp).UtcDateTime },
                        _ => existing with { LastCheckedDate = DateTimeOffset.FromUnixTimeSeconds(modUpdate.LatestFileUpdateTimestamp).UtcDateTime }
                    };
                    await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsFileUpdateEntity>(x => x.NexusModsModId == modUpdate.Id, ApplyChanges, context.CancellationToken);
                    processed++;
                }
            }
            finally
            {
                context.Result = $"Processed {processed} file updates";
                context.SetIsSuccess(true);
            }
        }
    }
}