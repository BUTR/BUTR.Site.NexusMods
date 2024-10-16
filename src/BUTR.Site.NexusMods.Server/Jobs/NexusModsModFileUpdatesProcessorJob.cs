using Bannerlord.ModuleManager;

using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Models.NexusModsAPI;
using BUTR.Site.NexusMods.Server.Options;
using BUTR.Site.NexusMods.Server.Repositories;
using BUTR.Site.NexusMods.Server.Services;

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
    private readonly NexusModsOptions _nexusModsOptions;
    private readonly INexusModsAPIClient _nexusModsAPIClient;
    private readonly INexusModsModFileParser _nexusModsModFileParser;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public NexusModsModFileUpdatesProcessorJob(ILogger<NexusModsModFileUpdatesProcessorJob> logger, IOptions<NexusModsOptions> nexusModsOptions, INexusModsAPIClient nexusModsAPIClient, INexusModsModFileParser nexusModsModFileParser, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _nexusModsOptions = nexusModsOptions.Value;
        _nexusModsAPIClient = nexusModsAPIClient;
        _nexusModsModFileParser = nexusModsModFileParser;
        _serviceScopeFactory = serviceScopeFactory;
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
            await using var scope = _serviceScopeFactory.CreateAsyncScope().WithTenant(tenant);
            var (processed_, exceptions_, updatesStoredWithinDay_, updatedWithinDay_, newUpdates_) = await HandleTenantAsync(scope, tenant, ct);
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

    private async Task<(int Processed, List<Exception> Exceptions, int UpdatesStoredWithinDay, int UpdatedWithinDay, int NewUpdates)> HandleTenantAsync(AsyncServiceScope scope, TenantId tenant, CancellationToken ct)
    {
        var unitOfWorkFactory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();
        await using var unitOfRead = unitOfWorkFactory.CreateUnitOfRead();
        await using var unitOfWrite = unitOfWorkFactory.CreateUnitOfWrite();

        var gameDomain = tenant.ToGameDomain();

        var dateOneWeekAgo = DateTime.UtcNow.AddDays(-7);
        var updatesStoredWithinWeek = await unitOfRead.NexusModsModToFileUpdates.GetAllAsync(x => x.LastCheckedDate > dateOneWeekAgo, null, ct);
        var updatedWithinWeek = await _nexusModsAPIClient.GetAllModUpdatesWeekAsync(gameDomain, _nexusModsOptions.ApiKey, ct) ?? Array.Empty<NexusModsUpdatedModsResponse>();
        var newUpdates = updatedWithinWeek.Where(x =>
        {
            var latestFileUpdateDate = DateTimeOffset.FromUnixTimeSeconds(x.LatestFileUpdateTimestamp).ToUniversalTime();
            if (latestFileUpdateDate < dateOneWeekAgo) return false;

            var found = updatesStoredWithinWeek.FirstOrDefault(y => y.NexusModsModId == x.Id);
            return found is null || found.LastCheckedDate < latestFileUpdateDate;
        }).ToList();

        var processed = 0;
        var exceptions = new List<Exception>();
        foreach (var modUpdate in newUpdates)
        {
            try
            {
                if (ct.IsCancellationRequested) break;

                if (await _nexusModsAPIClient.GetModFileInfosFullAsync(gameDomain, modUpdate.Id, _nexusModsOptions.ApiKey, ct) is not { } response) continue;

                var updates = response.FileUpdates.Where(x => DateTimeOffset.FromUnixTimeSeconds(x.UploadedTimestamp) > dateOneWeekAgo).ToArray();
                if (updates.Length == 0) continue;

                var infos = await _nexusModsModFileParser.GetModuleInfosAsync(gameDomain, modUpdate.Id, response.Files.Where(x => updates.Any(y => y.NewId == x.FileId)), _nexusModsOptions.ApiKey, ct).ToArrayAsync(ct);
                var lastUpdateTime = DateTimeOffset.FromUnixTimeSeconds(modUpdate.LatestFileUpdateTimestamp).ToUniversalTime();

                exceptions.AddRange(infos.Select(x => x.Exception).OfType<Exception>());

                unitOfWrite.NexusModsModModules.UpsertRange(infos.Select(x => x.ModuleInfo).OfType<ModuleInfoExtended>().DistinctBy(x => x.Id).Select(x => new NexusModsModToModuleEntity
                {
                    TenantId = tenant,
                    NexusModsModId = modUpdate.Id,
                    NexusModsMod = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsMod(modUpdate.Id),
                    ModuleId = ModuleId.From(x.Id),
                    Module = unitOfWrite.UpsertEntityFactory.GetOrCreateModule(ModuleId.From(x.Id)),
                    LastUpdateDate = lastUpdateTime,
                    LinkType = ModToModuleLinkType.ByUnverifiedFileExposure
                }).ToList());
                unitOfWrite.NexusModsModToFileUpdates.Upsert(new NexusModsModToFileUpdateEntity
                {
                    TenantId = tenant,
                    NexusModsModId = modUpdate.Id,
                    NexusModsMod = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsMod(modUpdate.Id),
                    LastCheckedDate = lastUpdateTime
                });
                unitOfWrite.NexusModsModToModuleInfoHistory.UpsertRange(infos.Where(x => x.ModuleInfo is not null).DistinctBy(x => new { x.ModuleInfo!.Id, x.ModuleInfo.Version, x.FileId }).Select(x => new NexusModsModToModuleInfoHistoryEntity
                {
                    TenantId = tenant,
                    NexusModsFileId = x.FileId,
                    NexusModsModId = modUpdate.Id,
                    NexusModsMod = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsMod(modUpdate.Id),
                    ModuleId = ModuleId.From(x.ModuleInfo!.Id),
                    Module = unitOfWrite.UpsertEntityFactory.GetOrCreateModule(ModuleId.From(x.ModuleInfo.Id)),
                    ModuleVersion = ModuleVersion.From(x.ModuleInfo.Version.ToString()),
                    ModuleInfo = ModuleInfoModel.Create(x.ModuleInfo),
                    UploadDate = x.Uploaded,
                }).ToList());

                await unitOfWrite.SaveChangesAsync(CancellationToken.None);
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