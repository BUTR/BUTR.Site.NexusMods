using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

public sealed record StatisticsCrashReport
{
    public required GameVersion GameVersion { get; init; }
    public required ModuleId ModuleId { get; init; }
    public required ModuleVersion ModuleVersion { get; init; }
    public required int InvolvedCount { get; init; }
    public required int NotInvolvedCount { get; init; }
    public required int TotalCount { get; init; }
    public required int Value { get; init; }
    public required double CrashScore { get; init; }
}

public interface ICrashReportToModuleMetadataEntityRepositoryRead : IRepositoryRead<CrashReportToModuleMetadataEntity>
{
    Task<IList<StatisticsCrashReport>> GetAllStatisticsAsync(CancellationToken ct);
}

public interface ICrashReportToModuleMetadataEntityRepositoryWrite : IRepositoryWrite<CrashReportToModuleMetadataEntity>, ICrashReportToModuleMetadataEntityRepositoryRead
{
    Task GenerateAutoCompleteForModuleIdsAsync(CancellationToken ct);
}