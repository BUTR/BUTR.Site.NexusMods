using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Options;
using BUTR.Site.NexusMods.Server.Repositories;
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
    private readonly NexusModsOptions _nexusModsOptions;
    private readonly INexusModsAPIClient _nexusModsAPIClient;
    private readonly INexusModsModFileParser _nexusModsModFileParser;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public NexusModsModFileProcessorJob(ILogger<NexusModsModFileProcessorJob> logger, IOptions<NexusModsOptions> nexusModsOptions, INexusModsAPIClient nexusModsAPIClient, INexusModsModFileParser nexusModsModFileParser, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _nexusModsOptions = nexusModsOptions.Value;
        _nexusModsAPIClient = nexusModsAPIClient;
        _nexusModsModFileParser = nexusModsModFileParser;
        _serviceScopeFactory = serviceScopeFactory;
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
            await using var scope = _serviceScopeFactory.CreateAsyncScope().WithTenant(tenant);
            var (processed_, exceptions_) = await HandleTenantAsync(scope, tenant, ct);
            processed += processed_;
            exceptions.AddRange(exceptions_);
        }

        context.Result = $"Processed {processed} files. Failed {exceptions.Count} files.{(exceptions.Count > 0 ? $"\n{string.Join('\n', exceptions)}" : string.Empty)}";
        context.SetIsSuccess(exceptions.Count == 0);
    }

    private async Task<(int Processed, List<Exception> Exceptions)> HandleTenantAsync(AsyncServiceScope scope, TenantId tenant, CancellationToken ct)
    {
        const int notFoundModsTreshold = 25;

        var unitOfWorkFactory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();
        await using var unitOfWrite = unitOfWorkFactory.CreateUnitOfWrite();

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
                var response = await _nexusModsAPIClient
                    .GetModFileInfosFullAsync(gameDomain, modId, _nexusModsOptions.ApiKey, ct);
                if (response is null)
                {
                    notFoundMods++;
                    modIdRaw++;
                    if (notFoundMods >= notFoundModsTreshold)
                        break;
                    continue;
                }

                var infos = await _nexusModsModFileParser.GetModuleInfosAsync(gameDomain, modId, response.Files, _nexusModsOptions.ApiKey, ct).ToArrayAsync(ct);
                var latestFileUpdate = DateTimeOffset.FromUnixTimeSeconds(response.Files.Select(x => x.UploadedTimestamp).Where(x => x is not null).Max() ?? 0).ToUniversalTime();

                nexusModsModModuleEntities.AddRange(infos.Select(x => x.ModuleInfo).DistinctBy(x => x.Id).Select(x => new NexusModsModToModuleEntity
                {
                    TenantId = tenant,
                    NexusModsModId = modId,
                    NexusModsMod = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsMod(modId),
                    ModuleId = ModuleId.From(x.Id),
                    Module = unitOfWrite.UpsertEntityFactory.GetOrCreateModule(ModuleId.From(x.Id)),
                    LastUpdateDate = latestFileUpdate,
                    LinkType = NexusModsModToModuleLinkType.ByUnverifiedFileExposure,
                }).ToList());
                nexusModsModToFileUpdateEntities.Add(new NexusModsModToFileUpdateEntity
                {
                    TenantId = tenant,
                    NexusModsModId = modId,
                    NexusModsMod = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsMod(modId),
                    LastCheckedDate = latestFileUpdate,
                });
                nexusModsModToModuleInfoHistoryEntities.AddRange(infos.DistinctBy(x => new { x.ModuleInfo.Id, x.ModuleInfo.Version, x.FileId }).Select(x => new NexusModsModToModuleInfoHistoryEntity
                {
                    TenantId = tenant,
                    NexusModsFileId = x.FileId,
                    NexusModsModId = modId,
                    NexusModsMod = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsMod(modId),
                    ModuleId = ModuleId.From(x.ModuleInfo.Id),
                    Module = unitOfWrite.UpsertEntityFactory.GetOrCreateModule(ModuleId.From(x.ModuleInfo.Id)),
                    ModuleVersion = ModuleVersion.From(x.ModuleInfo.Version.ToString()),
                    ModuleInfo = ModuleInfoModel.Create(x.ModuleInfo),
                    UploadDate = x.Uploaded,
                }).ToList());

                unitOfWrite.NexusModsModModules.UpsertRange(nexusModsModModuleEntities);
                unitOfWrite.NexusModsModToFileUpdates.UpsertRange(nexusModsModToFileUpdateEntities);
                unitOfWrite.NexusModsModToModuleInfoHistory.UpsertRange(nexusModsModToModuleInfoHistoryEntities);

                await unitOfWrite.SaveChangesAsync(ct);
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