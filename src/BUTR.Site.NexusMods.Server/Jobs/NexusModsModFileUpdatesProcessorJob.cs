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
        using var ctsTimeout = new CancellationTokenSource(TimeSpan.FromHours(6));
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, ctsTimeout.Token);
        var ct = cts.Token;

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

        var info = serviceProvider.GetRequiredService<NexusModsModFileParser>();
        var options = serviceProvider.GetRequiredService<IOptions<NexusModsOptions>>().Value;
        var client = serviceProvider.GetRequiredService<NexusModsAPIClient>();
        var dbContextRead = serviceProvider.GetRequiredService<IAppDbContextRead>();
        var dbContextWrite = serviceProvider.GetRequiredService<IAppDbContextWrite>();
        var entityFactory = dbContextWrite.GetEntityFactory();
        await using var _ = await dbContextWrite.CreateSaveScopeAsync();

        var dateOneWeekAgo = DateTime.UtcNow.AddDays(-7);
        var updatesStoredWithinWeek = await dbContextRead.NexusModsModToFileUpdates.Where(x => x.LastCheckedDate > dateOneWeekAgo).ToListAsync(ct);
        var updatedWithinWeek = await client.GetAllModUpdatesWeekAsync(gameDomain, options.ApiKey, ct) ?? Array.Empty<NexusModsUpdatedModsResponse>();
        var newUpdates = updatedWithinWeek.Where(x =>
        {
            var latestFileUpdateDate = DateTimeOffset.FromUnixTimeSeconds(x.LatestFileUpdateTimestamp).ToUniversalTime();
            if (latestFileUpdateDate < dateOneWeekAgo) return false;

            var found = updatesStoredWithinWeek.FirstOrDefault(y => y.NexusModsMod.NexusModsModId == x.Id);
            return found is null || found.LastCheckedDate < latestFileUpdateDate;
        }).ToList();

        var processed = 0;
        var exceptions = new List<Exception>();
        foreach (var modUpdate in newUpdates)
        {
            try
            {
                if (ct.IsCancellationRequested) break;

                if (await client.GetModFileInfosFullAsync(gameDomain, modUpdate.Id, options.ApiKey, ct) is not { } response) continue;

                var updates = response.FileUpdates.Where(x => DateTimeOffset.FromUnixTimeSeconds(x.UploadedTimestamp) > dateOneWeekAgo).ToArray();
                if (updates.Length == 0) continue;

                var infos = await info.GetModuleInfosAsync(gameDomain, modUpdate.Id, response.Files.Where(x => updates.Any(y => y.NewId == x.FileId)), options.ApiKey, ct).ToArrayAsync(ct);
                var lastUpdateTime = DateTimeOffset.FromUnixTimeSeconds(modUpdate.LatestFileUpdateTimestamp).ToUniversalTime();

                await dbContextWrite.NexusModsModModules.UpsertOnSaveAsync(infos.Select(x => x.ModuleInfo).DistinctBy(x => x.Id).Select(x => new NexusModsModToModuleEntity
                {
                    TenantId = tenant,
                    NexusModsMod = entityFactory.GetOrCreateNexusModsMod(modUpdate.Id),
                    Module = entityFactory.GetOrCreateModule(ModuleId.From(x.Id)),
                    LastUpdateDate = lastUpdateTime,
                    LinkType = NexusModsModToModuleLinkType.ByUnverifiedFileExposure
                }));
                await dbContextWrite.NexusModsModToFileUpdates.UpsertOnSaveAsync(new NexusModsModToFileUpdateEntity
                {
                    TenantId = tenant,
                    NexusModsMod = entityFactory.GetOrCreateNexusModsMod(modUpdate.Id),
                    LastCheckedDate = lastUpdateTime
                });
                await dbContextWrite.NexusModsModToModuleInfoHistory.UpsertOnSaveAsync(infos.DistinctBy(x => new { x.ModuleInfo.Id, x.ModuleInfo.Version, x.FileId }).Select(x => new NexusModsModToModuleInfoHistoryEntity
                {
                    TenantId = tenant,
                    NexusModsFileId = x.FileId,
                    NexusModsMod = entityFactory.GetOrCreateNexusModsMod(modUpdate.Id),
                    Module = entityFactory.GetOrCreateModule(ModuleId.From(x.ModuleInfo.Id)),
                    ModuleVersion = ModuleVersion.From(x.ModuleInfo.Version.ToString()),
                    ModuleInfo = ModuleInfoModel.Create(x.ModuleInfo),
                    UploadDate = x.Uploaded,
                }));
                processed++;
            }
            catch (Exception e)
            {
                exceptions.Add(new Exception($"Mod Id: {modUpdate.Id}", e));
            }
        }

        // Disposing the DBContext will save the data

        return (processed, exceptions, updatesStoredWithinWeek.Count, updatedWithinWeek.Length, newUpdates.Count);
    }
}