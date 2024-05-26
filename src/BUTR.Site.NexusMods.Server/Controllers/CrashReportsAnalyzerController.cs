using Bannerlord.ModuleManager;

using BUTR.CrashReport.Models;
using BUTR.CrashReport.Models.Analyzer;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Repositories;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.BindingSources;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), TenantRequired]
public sealed class CrashReportsAnalyzerController : ApiControllerBase
{
    public sealed record CrashReportDiagnosticsResult
    {
        public required ICollection<GenericSuggestedFix> Suggestions { get; init; }
        public required ICollection<ModuleSuggestedFix> ModuleSuggestions { get; init; }
        public required ICollection<ModuleUpdate> ModuleUpdates { get; init; }
    }


    private readonly ILogger _logger;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public CrashReportsAnalyzerController(ILogger<CrashReportsAnalyzerController> logger, IUnitOfWorkFactory unitOfWorkFactory)
    {
        _logger = logger;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    [HttpPost("Diagnostics")]
    public async Task<ActionResult<CrashReportDiagnosticsResult?>> GetDiagnosticsAsync([FromBody, Required] CrashReportModel crashReport, [BindTenant] TenantId tenant, [FromServices] ITenantContextAccessor tenantContextAccessor, CancellationToken ct)
    {
        tenantContextAccessor.Current = tenant;

        if (tenant == TenantId.Bannerlord)
        {
            return Ok(new CrashReportDiagnosticsResult
            {
                Suggestions = new List<GenericSuggestedFix>(),
                ModuleSuggestions = await GetModuleSuggestionsAsync(crashReport, ct).ToListAsync(ct),
                ModuleUpdates = await GetModuleUpdatesForBannerlordAsync(crashReport, ct).ToListAsync(ct),
            });
        }

        return Ok(new CrashReportDiagnosticsResult
        {
            Suggestions = new List<GenericSuggestedFix>(),
            ModuleSuggestions = new List<ModuleSuggestedFix>(),
            ModuleUpdates = new List<ModuleUpdate>(),
        });
    }

    private async IAsyncEnumerable<ModuleSuggestedFix> GetModuleSuggestionsAsync(CrashReportModel crashReport, [EnumeratorCancellation] CancellationToken ct)
    {
        static bool GetABIException(ref ExceptionModel? exceptionModel, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return false;

            if (exceptionModel is null)
                return false;

            if (exceptionModel.Type == "System.TypeLoadException")
                return true;

            if (exceptionModel.Type == "System.MissingFieldException")
                return true;

            if (exceptionModel.Type == "System.MissingMemberException")
                return true;

            if (exceptionModel.Type == "System.MissingMethodException")
                return true;

            exceptionModel = exceptionModel.InnerException;
            return GetABIException(ref exceptionModel, ct);
        }

        await Task.Yield();

        var exception = crashReport.Exception;
        if (GetABIException(ref exception, ct) && exception?.SourceModuleId is { } moduleId)
        {
            yield return new ModuleSuggestedFix
            {
                ModuleId = moduleId,
                Type = ModuleSuggestedFixType.Update,
            };
        }
    }

    private async IAsyncEnumerable<ModuleUpdate> GetModuleUpdatesForBannerlordAsync(CrashReportModel crashReport, [EnumeratorCancellation] CancellationToken ct)
    {
        await Task.Yield();

        if (!ApplicationVersion.TryParse(crashReport.Metadata.GameVersion, out var gVersion)) yield break;

        var currentModuleIdsWithoutAnyData = crashReport.Modules.Where(x => !x.IsOfficial && string.IsNullOrEmpty(x.Url) && x.UpdateInfo is null)
            .Select(x => ModuleId.From(x.Id)).ToArray();
        var currentMexusModsUpdateInfos = crashReport.Modules.Where(x => x is { IsOfficial: false, UpdateInfo.Provider: "NexusMods" })
            .Select(x => NexusModsModId.TryParse(x.UpdateInfo!.Value, out var modId) ? modId : NexusModsModId.None)
            .Where(x => x != NexusModsModId.None).ToArray();
        var currentNexusModsIds = crashReport.Modules.Where(x => !x.IsOfficial && !string.IsNullOrEmpty(x.Url))
            .Select(x => NexusModsModId.TryParseUrl(x.Url, out var modId) ? modId : NexusModsModId.None)
            .Where(x => x != NexusModsModId.None)
            .Except(currentMexusModsUpdateInfos).ToArray();

        var currentModules = crashReport.Modules
            .Where(x => !x.IsOfficial).Select(x => new { ModuleId = ModuleId.From(x.Id), Version = ModuleVersion.From(x.Version) }).ToArray();

        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        // SMAPI uses different update providers - Chucklefish, NexusMods, GitHub
        // We curectly will only use NexusMods
        //var updateInfoEntries = _dbContextRead.NexusModsModToModuleInfoHistory.Where(x => moduleIds.Contains(x.Module.ModuleId));
        //var entries = _dbContextRead.NexusModsModToModuleInfoHistory.Where(x => moduleIds.Contains(x.Module.ModuleId));
        var historicEntriesBasedOnModuleId = await unitOfRead.NexusModsModToModuleInfoHistory.GetAllAsync(x => currentModuleIdsWithoutAnyData.Contains(x.Module.ModuleId), null, ct);
        var historicEntriesBasedOnNexusModsId = await unitOfRead.NexusModsModToModuleInfoHistory.GetAllAsync(x => currentNexusModsIds.Contains(x.NexusModsMod.NexusModsModId), null, ct);
        var historicEntriesBasedOnUpdateInfo = await unitOfRead.NexusModsModToModuleInfoHistory.GetAllAsync(x => currentMexusModsUpdateInfos.Contains(x.NexusModsMod.NexusModsModId), null, ct);
        var allHistoricEntries = historicEntriesBasedOnModuleId.Concat(historicEntriesBasedOnNexusModsId).Concat(historicEntriesBasedOnUpdateInfo);

        var historicEntriesCompatibleWithGameVersion = allHistoricEntries.Select(x => new
        {
            ModuleId = x.Module.ModuleId,
            ModuleVersion = ApplicationVersion.TryParse(x.ModuleVersion.Value, out var v) ? v : ApplicationVersion.Empty,
            ModuleInfo = ModuleInfoModel.Create(x.ModuleInfo),
        }).Where(x => x.ModuleInfo.DependentModuleMetadatas.Any(y =>
        {
            // So how do we understand what game version the module requires?
            // Generally, any mod will need at least the Native module to work.
            // But there were mods like that Space Invader mod that will just require the engine - they still depend on Native.
            //
            // We also need to understand how versions work.
            // Version = v1.0.0 means that at least v1.0.0 is required
            // Version = v1.0.0-v1.2.0 means that at least v1.0.0 is required, but not higher than v1.2.0

            if (y.Id != "Native") return false;
            if (y.Version == ApplicationVersion.Empty && y.VersionRange == ApplicationVersionRange.Empty) return false;
            return (y.Version != ApplicationVersion.Empty && y.Version <= gVersion) ||
                   (y.VersionRange != ApplicationVersionRange.Empty && y.VersionRange.Min <= gVersion && y.VersionRange.Max > gVersion);
        })).ToArray();
        var latestModulesCompatibleWithGameVersions = historicEntriesCompatibleWithGameVersion
            .GroupBy(x => x.ModuleId)
            .Select(x => x.MaxBy(y => y.ModuleVersion, new ApplicationVersionComparer())!)
            .ToArray();

        var latestUpdates = latestModulesCompatibleWithGameVersions.Where(x =>
        {
            var tuple = currentModules.FirstOrDefault(y => y.ModuleId == x.ModuleId);
            if (tuple is null)
                return false;

            if (ApplicationVersion.TryParse(tuple.Version.Value, out var mVersion))
                return mVersion < x.ModuleVersion;
            return false;
        }).ToArray();

        foreach (var update in latestUpdates)
        {
            yield return new ModuleUpdate
            {
                ModuleId = update.ModuleId.Value,
                ModuleVersion = update.ModuleVersion.ToString(),
                IsModuleInvolved = crashReport.InvolvedModules.Any(y => y.ModuleOrLoaderPluginId == update.ModuleId)
            };
        }
    }
}