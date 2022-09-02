using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
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
    [DisallowConcurrentExecution]
    public sealed class NexusModsModFileProcessorJob : IJob
    {
        private readonly ILogger _logger;
        private readonly NexusModsOptions _options;
        private readonly NexusModsAPIClient _client;
        private readonly NexusModsInfo _info;
        private readonly AppDbContext _dbContext;

        public NexusModsModFileProcessorJob(ILogger<NexusModsModFileProcessorJob> logger, IOptions<NexusModsOptions> options, NexusModsAPIClient client, NexusModsInfo info, AppDbContext dbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _info = info ?? throw new ArgumentNullException(nameof(info));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var modId = 0;
            var updateDate = await _dbContext.Set<NexusModsFileUpdateEntity>().FirstOrDefaultAsync(x => x.NexusModsModId == modId, context.CancellationToken);
            var files = await _client.GetModFileInfosAsync("mountandblade2bannerlord", modId, _options.ApiKey);
            if (files is not null)
            {
                // max sequence no elements
                var latestFileUpdate = files.Files.Select(x => DateTimeOffset.FromUnixTimeSeconds(x.UploadedTimestamp).UtcDateTime).DefaultIfEmpty(DateTime.MinValue).Max();
                if (latestFileUpdate != DateTime.MinValue)
                {
                    if (updateDate is null || updateDate.LastCheckedDate < latestFileUpdate)
                    {
                        var exposedModIds = await _info.GetModIdsAsync("mountandblade2bannerlord", modId, _options.ApiKey).Distinct().ToImmutableArrayAsync(context.CancellationToken);

                        NexusModsExposedModsEntity? ApplyChanges2(NexusModsExposedModsEntity? existing) => existing switch
                        {
                            null => new() { NexusModsModId = modId, ModIds = exposedModIds.AsArray(), LastCheckedDate = DateTime.UtcNow },
                            var entity => entity with
                            {
                                ModIds = entity.ModIds.AsImmutableArray().AddRange(exposedModIds.Except(entity.ModIds)).AsArray(),
                                LastCheckedDate = DateTime.UtcNow
                            }
                        };
                        await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsExposedModsEntity>(x => x.NexusModsModId == modId, ApplyChanges2, context.CancellationToken);

                        NexusModsFileUpdateEntity? ApplyChanges(NexusModsFileUpdateEntity? existing) => existing switch
                        {
                            null => new() { NexusModsModId = modId, LastCheckedDate = latestFileUpdate },
                            var entity => entity with { LastCheckedDate = latestFileUpdate }
                        };
                        await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsFileUpdateEntity>(x => x.NexusModsModId == modId, ApplyChanges, context.CancellationToken);
                    }
                }
            }
            else
            {
                NexusModsExposedModsEntity? ApplyChanges2(NexusModsExposedModsEntity? existing) => existing switch
                {
                    _ => null
                };
                await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsExposedModsEntity>(x => x.NexusModsModId == modId, ApplyChanges2, context.CancellationToken);

                NexusModsFileUpdateEntity? ApplyChanges(NexusModsFileUpdateEntity? existing) => existing switch
                {
                    _ => null
                };
                await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsFileUpdateEntity>(x => x.NexusModsModId == modId, ApplyChanges, context.CancellationToken);
            }
        }
    }
}