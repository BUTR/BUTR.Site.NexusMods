using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Options;
using BUTR.Site.NexusMods.Server.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Quartz;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        using var ctsTimeout = new CancellationTokenSource(TimeSpan.FromDays(1));
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, ctsTimeout.Token);
        var ct = cts.Token;

        var processed = 0;
        var exceptions = new List<Exception>();
        foreach (var tenant in TenantId.Values)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();

            var tenantContextAccessor = scope.ServiceProvider.GetRequiredService<ITenantContextAccessor>();
            tenantContextAccessor.Current = tenant;

            var (processed_, exceptions_) = await HandleTenantAsync(tenant, scope.ServiceProvider, ct);
            processed += processed_;
            exceptions.AddRange(exceptions_);
        }

        context.Result = $"Processed {processed} files. Failed {exceptions.Count} files.{(exceptions.Count > 0 ? $"\n{string.Join('\n', exceptions)}" : string.Empty)}";
        context.SetIsSuccess(exceptions.Count == 0);
    }

    private static async Task<(int Processed, List<Exception> Exceptions)> HandleTenantAsync(TenantId tenant, IServiceProvider serviceProvider, CancellationToken ct)
    {
        const int notFoundModsTreshold = 25;

        var info = serviceProvider.GetRequiredService<INexusModsModFileParser>();
        var options = serviceProvider.GetRequiredService<IOptions<NexusModsOptions>>().Value;
        var client = serviceProvider.GetRequiredService<INexusModsAPIClient>();
        var dbContextWrite = serviceProvider.GetRequiredService<IAppDbContextWrite>();
        var entityFactory = dbContextWrite.GetEntityFactory();

        var gameDomain = tenant.ToGameDomain();

        var processed = 0;
        var exceptions = new List<Exception>();

        var notFoundMods = 0;
        var modIdRaw = 1; //2907, 5090
        while (!ct.IsCancellationRequested)
        {
            var modId = NexusModsModId.From(modIdRaw);

            var nexusModsModModuleEntities = ImmutableArray.CreateBuilder<NexusModsModToModuleEntity>();
            var nexusModsModToFileUpdateEntities = ImmutableArray.CreateBuilder<NexusModsModToFileUpdateEntity>();
            var nexusModsModToModuleInfoHistoryEntities = ImmutableArray.CreateBuilder<NexusModsModToModuleInfoHistoryEntity>();

            try
            {
                var response = await client.GetModFileInfosFullAsync(gameDomain, modId, options.ApiKey, ct);
                if (response is null)
                {
                    notFoundMods++;
                    modIdRaw++;
                    if (notFoundMods >= notFoundModsTreshold)
                        break;
                    continue;
                }

                var infos = await info.GetModuleInfosAsync(gameDomain, modId, response.Files, options.ApiKey, ct).ToArrayAsync(ct);
                var latestFileUpdate = DateTimeOffset.FromUnixTimeSeconds(response.Files.Select(x => x.UploadedTimestamp).Where(x => x is not null).Max() ?? 0).ToUniversalTime();

                nexusModsModModuleEntities.AddRange(infos.Select(x => x.ModuleInfo).DistinctBy(x => x.Id).Select(x => new NexusModsModToModuleEntity
                {
                    TenantId = tenant,
                    NexusModsMod = entityFactory.GetOrCreateNexusModsMod(modId),
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
                nexusModsModToModuleInfoHistoryEntities.AddRange(infos.DistinctBy(x => new { x.ModuleInfo.Id, x.ModuleInfo.Version, x.FileId }).Select(x => new NexusModsModToModuleInfoHistoryEntity
                {
                    TenantId = tenant,
                    NexusModsFileId = x.FileId,
                    NexusModsMod = entityFactory.GetOrCreateNexusModsMod(modId),
                    Module = entityFactory.GetOrCreateModule(ModuleId.From(x.ModuleInfo.Id)),
                    ModuleVersion = ModuleVersion.From(x.ModuleInfo.Version.ToString()),
                    ModuleInfo = ModuleInfoModel.Create(x.ModuleInfo),
                    UploadDate = x.Uploaded,
                }));

                await using var _ = await dbContextWrite.CreateSaveScopeAsync();
                await dbContextWrite.NexusModsModModules.UpsertOnSaveAsync(nexusModsModModuleEntities.ToArray());
                await dbContextWrite.NexusModsModToFileUpdates.UpsertOnSaveAsync(nexusModsModToFileUpdateEntities.ToArray());
                await dbContextWrite.NexusModsModToModuleInfoHistory.UpsertOnSaveAsync(nexusModsModToModuleInfoHistoryEntities.ToArray());
                // Disposing the DBContext will save the data

                processed++;
            }
            catch (Exception e)
            {
                exceptions.Add(new Exception($"Mod Id: {modId}", e));
            }

            modIdRaw++;
        }

        return (processed, exceptions);
    }
}