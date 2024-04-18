using Bannerlord.ModuleManager;

using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.BindingSources;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), TenantRequired]
public sealed class ModsAnalyzerController : ApiControllerBase
{
    public sealed record CompatibilityScoreRequestModuleEntry
    {
        public required ModuleId ModuleId { get; init; }
        public ModuleVersion ModuleVersion { get; init; } = ModuleVersion.From(string.Empty);
    }
    public sealed record CompatibilityScoreRequest
    {
        public required GameVersion GameVersion { get; init; }
        public required ICollection<CompatibilityScoreRequestModuleEntry> Modules { get; init; }
    }


    public sealed record CompatibilityScoreResultModuleEntry
    {
        public required ModuleId ModuleId { get; init; }
        public required double Compatibility { get; init; }

        public required ModuleVersion? RecommendedModuleVersion { get; init; }
    }
    public sealed record CompatibilityScoreResult
    {
        public required ICollection<CompatibilityScoreResultModuleEntry> Modules { get; init; }
    }


    private readonly ILogger _logger;
    private readonly IAppDbContextRead _dbContextRead;

    public ModsAnalyzerController(ILogger<ModsAnalyzerController> logger, IAppDbContextRead dbContextRead)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContextRead = dbContextRead ?? throw new ArgumentNullException(nameof(dbContextRead));
    }

    [HttpPost("GetCompatibilityScore")]
    public async Task<ActionResult<CompatibilityScoreResult?>> GetCompatibilityScoreAsync([BindTenant] TenantId tenant, [FromBody] CompatibilityScoreRequest crashReport, [FromServices] ITenantContextAccessor tenantContextAccessor, CancellationToken ct)
    {
        tenantContextAccessor.Current = tenant;

        if (tenant == TenantId.Bannerlord)
        {
            var toExclude = new HashSet<ModuleId>()
            {
                ModuleId.From("Native"),
                ModuleId.From("SandBoxCore"),
                ModuleId.From("Sandbox"),
                ModuleId.From("StoryMode"),
                ModuleId.From("BirthAndDeath"),
                ModuleId.From("CustomBattle"),
                ModuleId.From("Multiplayer"),
            };
            var comparer = new ApplicationVersionComparer();
            return Ok(new CompatibilityScoreResult
            {
                Modules = await GetCompatibilityScoresAsync(crashReport with
                {
                    Modules = crashReport.Modules.Where(x => !toExclude.Contains(x.ModuleId)).ToArray(),
                },
                (x, y) =>
                {
                    return y
                        ? x.OrderBy(z => ApplicationVersion.TryParse(z.Value, out var v) ? v : ApplicationVersion.Empty, comparer)
                        : x.OrderByDescending(z => ApplicationVersion.TryParse(z.Value, out var v) ? v : ApplicationVersion.Empty, comparer);
                },
                (x, y) =>
                {
                    var avX = ApplicationVersion.TryParse(x.Value, out var vX) ? vX : ApplicationVersion.Empty;
                    var avY = ApplicationVersion.TryParse(y.Value, out var vY) ? vY : ApplicationVersion.Empty;
                    return comparer.Compare(avX, avY);
                }, ct).ToListAsync(ct),
            });
        }

        return Ok(new CompatibilityScoreResult
        {
            Modules = new List<CompatibilityScoreResultModuleEntry>(),
        });
    }

    private async IAsyncEnumerable<CompatibilityScoreResultModuleEntry> GetCompatibilityScoresAsync(
        CompatibilityScoreRequest data,
        Func<IEnumerable<ModuleVersion>, bool, IOrderedEnumerable<ModuleVersion>> orderBy,
        Func<ModuleVersion, ModuleVersion, int> comparer,
        [EnumeratorCancellation] CancellationToken ct)
    {
        await Task.Yield();

        var gameVersion = data.GameVersion;
        var currentModules = data.Modules;

        var moduleIds = currentModules.Select(x => x.ModuleId).ToArray();
        var allRawScoresForAllModules = await _dbContextRead.StatisticsCrashScoreInvolveds
            .Where(x => x.GameVersion == gameVersion && moduleIds.Contains(x.Module.ModuleId))
            .GroupBy(x => x.Module.ModuleId)
            .Select(x => new
            {
                ModuleId = x.Key,
                // We order by score so that we can take the top 10
                RawScores = x.OrderBy(y => y.Score).Select(y => new
                {
                    ModuleVersion = y.ModuleVersion,
                    RawScore = y.Score,
                    TotalCount = y.TotalCount,
                }).Take(10).ToArray(),
            }).ToArrayAsync(ct);

        var allScoresForAllModules = allRawScoresForAllModules.Select(x =>
        {
            var moduleVersionsOrdered = orderBy(x.RawScores.Select(y => y.ModuleVersion), true).ToArray();
            var totalCountMax = x.RawScores.Max(y => y.TotalCount);
            return new
            {
                ModuleId = x.ModuleId,
                Scores = x.RawScores.Select(y => new
                {
                    ModuleVersion = y.ModuleVersion,
                    RawScore = y.RawScore,
                    WeightTotalCount = (double) y.TotalCount / (double) totalCountMax,
                    WeightModuleVersion = (double) (Array.IndexOf(moduleVersionsOrdered, y.ModuleVersion) + 1) / (double) moduleVersionsOrdered.Length,
                }).Select(y => new
                {
                    ModuleVersion = y.ModuleVersion,
                    ScoreWeighted = 1d - (y.RawScore * y.WeightTotalCount * y.WeightModuleVersion),
                }).ToArray(),
            };
        }).ToArray();

        const double threshold = 0.15;
        foreach (var module in allScoresForAllModules)
        {
            var bestScore = module.Scores.Max(x => x.ScoreWeighted);

            var currentVersion = currentModules.First(x => x.ModuleId == module.ModuleId).ModuleVersion;
            var currentVersionScore = module.Scores.FirstOrDefault(x => x.ModuleVersion == currentVersion)?.ScoreWeighted ?? 0d;

            var versionsWithinBestScoreTreshold = module.Scores.Where(x => Math.Abs(x.ScoreWeighted - bestScore) <= threshold);
            var recommendedVersion = orderBy(versionsWithinBestScoreTreshold.Select(x => x.ModuleVersion), false).First();
            var recommendedVersionScore = module.Scores.First(x => x.ModuleVersion == recommendedVersion).ScoreWeighted;

            if (comparer(currentVersion, recommendedVersion) >= 0 && Math.Abs(currentVersionScore - bestScore) <= threshold)
            {
                // If the recommended version is older that this, but this is sthill within the treshold, do not recommend the old version
                yield return new CompatibilityScoreResultModuleEntry
                {
                    ModuleId = module.ModuleId,
                    Compatibility = Math.Round(currentVersionScore * 100, 2, MidpointRounding.ToNegativeInfinity),
                    RecommendedModuleVersion = null,
                };
            }
            else
            {
                // If the recommended version is newer than this, or this is outside the treshold, recommend the new version
                yield return new CompatibilityScoreResultModuleEntry
                {
                    ModuleId = module.ModuleId,
                    Compatibility = Math.Round(recommendedVersionScore * 100, 2),
                    RecommendedModuleVersion = recommendedVersion,
                };
            }
        }
    }
}