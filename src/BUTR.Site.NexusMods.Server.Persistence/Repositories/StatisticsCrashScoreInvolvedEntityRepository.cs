using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

public sealed record VersionScoreModel
{
    public required ModuleVersion Version { get; init; }
    public required double Score { get; init; }
    public required double Value { get; init; }
    public required int CountStable { get; init; }
    public required int CountUnstable { get; init; }
    public double Count => CountStable + CountUnstable;
}
public sealed record VersionStorageModel
{
    public required ModuleVersion Version { get; init; }
    public required VersionScoreModel[] Scores { get; init; }
    public double MeanScore => Scores.Length == 0 ? 0 : 1 - (Scores.Sum(x => x.Value) / (double) Scores.Sum(x => x.Count));
};
public sealed record ModuleStorageModel
{
    public required ModuleId ModuleId { get; init; }
    public required VersionStorageModel[] Versions { get; init; }
};
public sealed record StatisticsInvolvedModuleScoresForGameVersionModel
{
    public required GameVersion GameVersion { get; init; }
    public required ModuleStorageModel[] Modules { get; init; }
}

public sealed record RawScoreForModuleVersionModel
{
    public required ModuleVersion ModuleVersion { get; init; }
    public required double RawScore { get; init; }
    public required int TotalCount { get; init; }
}

public sealed record StatisticsRawScoresForModuleModel
{
    public required ModuleId ModuleId { get; init; }
    public required RawScoreForModuleVersionModel[] RawScores { get; init; }
}

public interface IStatisticsCrashScoreInvolvedEntityRepositoryRead : IRepositoryRead<StatisticsCrashScoreInvolvedEntity>
{
    Task<IList<StatisticsInvolvedModuleScoresForGameVersionModel>> GetAllInvolvedModuleScoresForGameVersionAsync(GameVersion[]? gameVersions, ModuleId[]? moduleIds, ModuleVersion[]? moduleVersions, CancellationToken ct);
    Task<IList<StatisticsRawScoresForModuleModel>> GetAllRawScoresForAllModulesAsync(GameVersion gameVersion, ModuleId[] moduleIds, CancellationToken ct);

}
public interface IStatisticsCrashScoreInvolvedEntityRepositoryWrite : IRepositoryWrite<StatisticsCrashScoreInvolvedEntity>, IStatisticsCrashScoreInvolvedEntityRepositoryRead;