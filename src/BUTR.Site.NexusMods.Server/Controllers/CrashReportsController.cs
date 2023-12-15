using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.BindingSources;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;
using BUTR.Site.NexusMods.Server.Utils.Http.StreamingMultipartResults;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[TenantNotRequired]
[ApiController, Route("api/v1/[controller]"), ButrNexusModsAuthorization, TenantRequired]
public sealed class CrashReportsController : ApiControllerBase
{
    public sealed record CrashReportModel2
    {
        public required CrashReportId Id { get; init; }
        public required CrashReportVersion Version { get; init; }
        public required GameVersion GameVersion { get; init; }
        public required ExceptionTypeId ExceptionType { get; init; }
        public required string Exception { get; init; }
        public required DateTimeOffset Date { get; init; }
        public required CrashReportUrl Url { get; init; }
        public required ImmutableArray<ModuleId> InvolvedModules { get; init; }
        public CrashReportStatus Status { get; init; } = CrashReportStatus.New;
        public string Comment { get; init; } = string.Empty;
    }

    private sealed record ModuleIdToVersionView
    {
        public required ModuleId ModuleId { get; init; }
        public required ModuleVersion Version { get; init; }
    }
    private sealed record UserCrashReportView
    {
        public required CrashReportId Id { get; init; }
        public required CrashReportVersion Version { get; init; }
        public required GameVersion GameVersion { get; init; }
        public required ExceptionTypeId ExceptionType { get; init; }
        public required string Exception { get; init; }
        public required DateTimeOffset CreatedAt { get; init; }
        public required ModuleId[] ModuleIds { get; init; }
        public required ModuleIdToVersionView[] ModuleIdToVersion { get; init; }
        public required ModuleId? TopInvolvedModuleId { get; init; }
        public required ModuleId[] InvolvedModuleIds { get; init; }
        public required NexusModsModId[] NexusModsModIds { get; init; }
        public required CrashReportUrl Url { get; init; }

        public required CrashReportStatus Status { get; init; }
        public required string? Comment { get; init; }
    }

    private readonly ILogger _logger;
    private readonly IAppDbContextRead _dbContextRead;
    private readonly IAppDbContextWrite _dbContextWrite;

    public CrashReportsController(ILogger<CrashReportsController> logger, IAppDbContextRead dbContextRead, IAppDbContextWrite dbContextWrite)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContextRead = dbContextRead ?? throw new ArgumentNullException(nameof(dbContextRead));
        _dbContextWrite = dbContextWrite ?? throw new ArgumentNullException(nameof(dbContextWrite));
    }

    private async Task<BasePaginated<UserCrashReportView, CrashReportModel2>> PaginatedBaseAsync(PaginatedQuery query, NexusModsUserId userId, CancellationToken ct)
    {
        var page = query.Page;
        var pageSize = Math.Max(Math.Min(query.PageSize, 50), 5);
        var filters = query.Filters ?? Enumerable.Empty<Filtering>();
        var sortings = query.Sotings is null || query.Sotings.Count == 0
            ? new List<Sorting> { new() { Property = nameof(CrashReportEntity.CreatedAt), Type = SortingType.Descending } }
            : query.Sotings;

        var user = await _dbContextRead.NexusModsUsers
            .Include(x => x.ToModules).ThenInclude(x => x.Module)
            .Include(x => x.ToNexusModsMods).ThenInclude(x => x.NexusModsMod)
            .AsSplitQuery()
            .FirstAsync(x => x.NexusModsUserId == userId, ct);

        IQueryable<UserCrashReportView> DbQueryBase(Expression<Func<CrashReportEntity, bool>> predicate)
        {
            return _dbContextRead.CrashReports
                .Include(x => x.ToUsers).ThenInclude(x => x.NexusModsUser)
                .Include(x => x.ModuleInfos).ThenInclude(x => x.Module)
                .Include(x => x.ModuleInfos).ThenInclude(x => x.NexusModsMod)
                .Include(x => x.ModuleInfos).ThenInclude(x => x.Module)
                .Include(x => x.ExceptionType)
                .AsSplitQuery()
                .Where(predicate)
                .Select(x => new UserCrashReportView
                {
                    Id = x.CrashReportId,
                    Version = x.Version,
                    GameVersion = x.GameVersion,
                    ExceptionType = x.ExceptionType.ExceptionTypeId,
                    Exception = x.Exception,
                    CreatedAt = x.CreatedAt,
                    Url = x.Url,
                    ModuleIds = x.ModuleInfos.Select(y => y.Module).Select(y => y.ModuleId).ToArray(),
                    ModuleIdToVersion = x.ModuleInfos.Select(y => new ModuleIdToVersionView { ModuleId = y.Module.ModuleId, Version = y.Version }).ToArray(),
                    TopInvolvedModuleId = x.ModuleInfos.Where(z => z.IsInvolved).Select(y => y.Module).Select(y => y.ModuleId).Cast<ModuleId?>().FirstOrDefault(),
                    InvolvedModuleIds = x.ModuleInfos.Where(z => z.IsInvolved).Select(y => y.Module).Select(y => y.ModuleId).ToArray(),
                    NexusModsModIds = x.ModuleInfos.Select(y => y.NexusModsMod).Where(y => y != null).Select(y => y!.NexusModsModId).ToArray(),
                    Status = x.ToUsers.Where(y => y.NexusModsUser.NexusModsUserId == userId).Select(y => y.Status).FirstOrDefault(),
                    Comment = x.ToUsers.Where(y => y.NexusModsUser.NexusModsUserId == userId).Select(y => y.Comment).FirstOrDefault(),
                })
                .WithFilter(filters)
                .WithSort(sortings);
        }

        var moduleIds = user.ToModules.Select(x => x.Module.ModuleId).ToHashSet();
        var nexusModsModIds = user.ToNexusModsMods.Select(x => x.NexusModsMod.NexusModsModId).ToHashSet();

        var dbQuery = User.IsInRole(ApplicationRoles.Administrator) || User.IsInRole(ApplicationRoles.Moderator)
            ? DbQueryBase(x => true)
            : DbQueryBase(x => x.ToUsers.Any(y => y.NexusModsUser.NexusModsUserId == userId) ||
                               x.ModuleInfos.Any(y => moduleIds.Contains(y.Module.ModuleId)) ||
                               x.ModuleInfos.Where(y => y.NexusModsMod != null).Any(y => nexusModsModIds.Contains(y.NexusModsMod!.NexusModsModId)));

        return new(await dbQuery.PaginatedAsync(page, pageSize, ct), items => items.Select(x => new CrashReportModel2
        {
            Id = x.Id,
            Version = x.Version,
            GameVersion = x.GameVersion,
            ExceptionType = x.ExceptionType,
            Exception = x.Exception,
            Date = x.CreatedAt,
            Url = x.Url,
            InvolvedModules = x.InvolvedModuleIds.ToImmutableArray(),
            Status = x.Status,
            Comment = x.Comment ?? string.Empty,
        }));
    }

    [HttpPost("Paginated")]
    public async Task<ApiResult<PagingData<CrashReportModel2>?>> PaginatedAsync([FromBody] PaginatedQuery query, [BindUserId] NexusModsUserId userId, CancellationToken ct)
    {
        var (paginated, transform) = await PaginatedBaseAsync(query, userId, ct);
        return ApiPagingResult(paginated, transform);
    }

    [HttpPost("PaginatedStreaming")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<StreamingMultipartResult> PaginatedStreamingAsync([FromBody] PaginatedQuery query, [BindUserId] NexusModsUserId userId, CancellationToken ct)
    {
        var (paginated, transform) = await PaginatedBaseAsync(query, userId, ct);
        return ApiPagingStreamingResult(paginated, transform);
    }

    [HttpGet("Autocomplete")]
    public ApiResult<IQueryable<string>?> Autocomplete([FromQuery] ModuleId modId)
    {
        return ApiResult(_dbContextRead.AutocompleteStartsWith<CrashReportToModuleMetadataEntity, ModuleId>(x => x.Module.ModuleId, modId));
    }

    [HttpPost("Update")]
    public async Task<ApiResult<string?>> UpdateAsync([FromBody] CrashReportModel2 updatedCrashReport, [BindUserId] NexusModsUserId userId, [BindTenant] TenantId tenant)
    {
        var entityFactory = _dbContextWrite.GetEntityFactory();
        await using var _ = await _dbContextWrite.CreateSaveScopeAsync();

        var entity = new NexusModsUserToCrashReportEntity
        {
            TenantId = tenant,
            NexusModsUser = entityFactory.GetOrCreateNexusModsUser(userId),
            CrashReportId = updatedCrashReport.Id,
            Status = updatedCrashReport.Status,
            Comment = updatedCrashReport.Comment
        };
        await _dbContextWrite.NexusModsUserToCrashReports.UpsertOnSaveAsync(entity);
        return ApiResult("Updated successful!");
    }
}