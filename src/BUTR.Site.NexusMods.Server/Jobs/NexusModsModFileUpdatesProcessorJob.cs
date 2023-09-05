using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Models.NexusModsAPI;
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
/// Will be able to keep the database consistent as long as the service is not stopped for more than a day
/// </summary>
[DisallowConcurrentExecution]
public sealed class NexusModsModFileUpdatesProcessorJob : IJob
{
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public NexusModsModFileUpdatesProcessorJob(ILogger<NexusModsModFileUpdatesProcessorJob> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var ct = context.CancellationToken;

        var processed = 0;
        var exceptions = new List<Exception>();
        var updatesStoredWithinDay = 0;
        var updatedWithinDay = 0;
        var newUpdates = 0;
        foreach (var tenant in TenantId.Values)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();

            var tenantContextAccessor = scope.ServiceProvider.GetRequiredService<ITenantContextAccessor>();
            tenantContextAccessor.Current = tenant;

            var (processed_, exceptions_, updatesStoredWithinDay_, updatedWithinDay_, newUpdates_) = await HandleTenantAsync(tenant, scope.ServiceProvider, ct);
            processed += processed_;
            exceptions.AddRange(exceptions_);
            updatesStoredWithinDay += updatesStoredWithinDay_;
            updatedWithinDay += updatedWithinDay_;
            newUpdates += newUpdates_;
        }

        context.MergedJobDataMap["UpdatesStoredWithinDay"] = updatesStoredWithinDay;
        context.MergedJobDataMap["UpdatedWithinDay"] = updatedWithinDay;
        context.MergedJobDataMap["NewUpdates"] = newUpdates;

        context.Result = $"Processed {processed} file updates. Failed {exceptions.Count} file updates.{(exceptions.Count > 0 ? $"\n{string.Join('\n', exceptions)}" : string.Empty)}";
        context.SetIsSuccess(exceptions.Count == 0);
    }

    private static async Task<(int Processed, List<Exception> Exceptions, int UpdatesStoredWithinDay, int UpdatedWithinDay, int NewUpdates)> HandleTenantAsync(TenantId tenant, IServiceProvider serviceProvider, CancellationToken ct)
    {
        var gameDomain = tenant.ToGameDomain();

        var info = serviceProvider.GetRequiredService<NexusModsInfo>();
        var options = serviceProvider.GetRequiredService<IOptions<NexusModsOptions>>().Value;
        var client = serviceProvider.GetRequiredService<NexusModsAPIClient>();
        var dbContextRead = serviceProvider.GetRequiredService<IAppDbContextRead>();
        var dbContextWrite = serviceProvider.GetRequiredService<IAppDbContextWrite>();
        var entityFactory = dbContextWrite.CreateEntityFactory();
        await using var _ = dbContextWrite.CreateSaveScope();

        var updatesStoredWithinDay = await dbContextRead.NexusModsModToFileUpdates.Where(x => x.LastCheckedDate > DateTime.UtcNow.AddDays(-1)).ToListAsync(ct);
        var updatedWithinDay = await client.GetAllModUpdatesAsync(gameDomain, options.ApiKey, ct) ?? Array.Empty<NexusModsUpdatedModsResponse>();
        var newUpdates = updatedWithinDay.Where(x =>
        {
            var latestFileUpdateDate = DateTimeOffset.FromUnixTimeSeconds(x.LatestFileUpdateTimestamp).UtcDateTime;
            if (latestFileUpdateDate < DateTime.UtcNow.AddDays(-1)) return false;

            var found = updatesStoredWithinDay.FirstOrDefault(y => y.NexusModsMod.NexusModsModId == x.Id);
            return found is null || found.LastCheckedDate < latestFileUpdateDate;
        }).ToList();

        var processed = 0;
        var exceptions = new List<Exception>();
        var nexusModsModModuleEntities = new List<NexusModsModToModuleEntity>();
        var nexusModsModToFileUpdateEntities = new List<NexusModsModToFileUpdateEntity>();
        foreach (var modUpdate in newUpdates)
        {
            try
            {
                if (ct.IsCancellationRequested) break;

                var exposedModIds = await info.GetModIdsAsync(gameDomain, modUpdate.Id, options.ApiKey, ct).Distinct().ToImmutableArrayAsync(ct);
                var lastUpdateTime = DateTimeOffset.FromUnixTimeSeconds(modUpdate.LatestFileUpdateTimestamp).UtcDateTime;

                nexusModsModModuleEntities.AddRange(exposedModIds.Select(x => new NexusModsModToModuleEntity
                {
                    TenantId = tenant,
                    NexusModsMod = entityFactory.GetOrCreateNexusModsMod(modUpdate.Id),
                    Module = entityFactory.GetOrCreateModule(x),
                    LastUpdateDate = lastUpdateTime,
                    LinkType = NexusModsModToModuleLinkType.ByUnverifiedFileExposure
                }));
                nexusModsModToFileUpdateEntities.Add(new NexusModsModToFileUpdateEntity
                {
                    TenantId = tenant,
                    NexusModsMod = entityFactory.GetOrCreateNexusModsMod(modUpdate.Id),
                    LastCheckedDate = lastUpdateTime
                });
                processed++;
            }
            catch (Exception e)
            {
                exceptions.Add(new Exception($"Mod Id: {modUpdate.Id}", e));
            }
        }

        dbContextWrite.FutureUpsert(x => x.NexusModsModModules, nexusModsModModuleEntities);
        dbContextWrite.FutureUpsert(x => x.NexusModsModToFileUpdates, nexusModsModToFileUpdateEntities);
        // Disposing the DBContext will save the data

        return (processed, exceptions, updatesStoredWithinDay.Count, updatedWithinDay.Length, newUpdates.Count);
    }
}