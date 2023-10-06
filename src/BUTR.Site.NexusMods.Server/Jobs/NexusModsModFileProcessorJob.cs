using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Options;
using BUTR.Site.NexusMods.Server.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Quartz;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Jobs;

/// <summary>
/// First time job to fill the database
/// </summary>
[DisallowConcurrentExecution]
public sealed class NexusModsModFileProcessorJob : IJob
{
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public NexusModsModFileProcessorJob(ILogger<NexusModsModFileProcessorJob> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var ct = context.CancellationToken;

        foreach (var tenant in TenantId.Values)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();

            var tenantContextAccessor = scope.ServiceProvider.GetRequiredService<ITenantContextAccessor>();
            tenantContextAccessor.Current = tenant;

            await HandleTenantAsync(tenant, scope.ServiceProvider, ct);
        }

        context.Result = "Finished processing all available files";
        context.SetIsSuccess(true);
    }

    private static async Task HandleTenantAsync(TenantId tenant, IServiceProvider serviceProvider, CancellationToken ct)
    {
        const int notFoundModIdsTreshold = 25;

        var info = serviceProvider.GetRequiredService<NexusModsModFileParser>();
        var options = serviceProvider.GetRequiredService<IOptions<NexusModsOptions>>().Value;
        var client = serviceProvider.GetRequiredService<NexusModsAPIClient>();
        var dbContextRead = serviceProvider.GetRequiredService<IAppDbContextRead>();
        var dbContextWrite = serviceProvider.GetRequiredService<IAppDbContextWrite>();
        var entityFactory = dbContextWrite.CreateEntityFactory();

        var gameDomain = tenant.ToGameDomain();

        var modIdRaw = 0;
        var notFoundModIds = 0;
        var @break = false;
        while (!ct.IsCancellationRequested && !@break)
        {
            await using var _ = dbContextWrite.CreateSaveScope();
            var nexusModsModModuleEntities = new List<NexusModsModToModuleEntity>();
            var nexusModsModToFileUpdateEntities = new List<NexusModsModToFileUpdateEntity>();
            var nexusModsModToModuleInfoHistoryEntities = new List<NexusModsModToModuleInfoHistoryEntity>();
            
            for (var i = 0; i < 50; i++)
            {
                var modId = NexusModsModId.From(modIdRaw);

                var updateDate = await dbContextRead.NexusModsModToFileUpdates.FirstOrDefaultAsync(x => x.NexusModsMod.NexusModsModId == modId, ct);
                if (await client.GetModFileInfosAsync(gameDomain, modId, options.ApiKey, ct) is not { } files)
                {
                    notFoundModIds++;
                    modIdRaw++;
                    if (notFoundModIds >= notFoundModIdsTreshold)
                    {
                        @break = true;
                        break;
                    }
                    continue;
                }
                notFoundModIds = 0;

                // max sequence no elements
                var latestFileUpdate = files.Files.Select(x => DateTimeOffset.FromUnixTimeSeconds(x.UploadedTimestamp).UtcDateTime).DefaultIfEmpty(DateTime.MinValue).Max();
                if (latestFileUpdate != DateTime.MinValue)
                {
                    //if (updateDate is null || updateDate.LastCheckedDate < latestFileUpdate)
                    {
                        var exposedModuleInfos = await info.GetModuleInfosAsync(gameDomain, modId, options.ApiKey, ct).ToArrayAsync(ct);

                        var id = modIdRaw;
                        nexusModsModModuleEntities.AddRange(exposedModuleInfos.DistinctBy(x => x.Id).Select(x => new NexusModsModToModuleEntity
                        {
                            TenantId = tenant,
                            NexusModsMod = entityFactory.GetOrCreateNexusModsMod(NexusModsModId.From(id)),
                            Module = entityFactory.GetOrCreateModule(ModuleId.From(x.Id)),
                            LastUpdateDate = latestFileUpdate,
                            LinkType = NexusModsModToModuleLinkType.ByUnverifiedFileExposure,
                        }));
                        nexusModsModToFileUpdateEntities.Add(new NexusModsModToFileUpdateEntity
                        {
                            TenantId = tenant,
                            NexusModsMod = entityFactory.GetOrCreateNexusModsMod(modId),
                            LastCheckedDate = latestFileUpdate,
                        });
                        nexusModsModToModuleInfoHistoryEntities.AddRange(exposedModuleInfos.DistinctBy(x => new { x.Id, x.Version }).Select(x => new NexusModsModToModuleInfoHistoryEntity
                        {
                            TenantId = tenant,
                            NexusModsMod = entityFactory.GetOrCreateNexusModsMod(modId),
                            Module = entityFactory.GetOrCreateModule(ModuleId.From(x.Id)),
                            ModuleVersion = ModuleVersion.From(x.Version.ToString()),
                            ModuleInfo = ModuleInfoModel.Create(x),
                        }));
                    }
                }
                
                modIdRaw++;
            }

            dbContextWrite.FutureUpsert(x => x.NexusModsModModules, nexusModsModModuleEntities);
            dbContextWrite.FutureUpsert(x => x.NexusModsModToFileUpdates, nexusModsModToFileUpdateEntities);
            dbContextWrite.FutureUpsert(x => x.NexusModsModToModuleInfoHistory, nexusModsModToModuleInfoHistoryEntities);
            // Disposing the DBContext will save the data
        }
    }
}