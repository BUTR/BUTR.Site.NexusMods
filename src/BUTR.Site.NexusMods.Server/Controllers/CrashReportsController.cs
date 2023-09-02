using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Utils.Http.StreamingJson;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Authorization;
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

[ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
public sealed class CrashReportsController : ControllerExtended
{
    public sealed record CrashReportModel
    {
        public required Guid Id { get; init; }
        public required int Version { get; init; }
        public required string GameVersion { get; init; }
        public required string ExceptionType { get; init; }
        public required string Exception { get; init; }
        public required DateTime Date { get; init; }
        public required string Url { get; init; }
        public required ImmutableArray<string> InvolvedModules { get; init; }
        public CrashReportStatus Status { get; init; } = CrashReportStatus.New;
        public string Comment { get; init; } = string.Empty;
    }

    private sealed record ModuleIdToVersionView
    {
        public required string ModuleId { get; init; }
        public required string Version { get; init; }
    }
    private sealed record UserCrashReportView
    {
        public required Guid Id { get; init; }
        public required int Version { get; init; }
        public required string GameVersion { get; init; }
        public required string ExceptionType { get; init; }
        public required string Exception { get; init; }
        public required DateTime CreatedAt { get; init; }
        public required string[] ModuleIds { get; init; }
        public required ModuleIdToVersionView[] ModuleIdToVersion { get; init; }
        public required string? TopInvolvedModuleId { get; init; }
        public required string[] InvolvedModuleIds { get; init; }
        public required int[] NexusModsModIds { get; init; }
        public required string Url { get; init; }

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

    private async Task<BasePaginated<UserCrashReportView, CrashReportModel>> PaginatedBaseAsync(PaginatedQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var pageSize = Math.Max(Math.Min(query.PageSize, 50), 5);
        var filters = query.Filters ?? Enumerable.Empty<Filtering>();
        var sortings = query.Sotings is null || query.Sotings.Count == 0
            ? new List<Sorting> { new() { Property = nameof(CrashReportEntity.CreatedAt), Type = SortingType.Descending } }
            : query.Sotings;

        var userId = HttpContext.GetUserId();

        var user = await _dbContextRead.NexusModsUsers
            .Include(x => x.ToModules)
            .ThenInclude(x => x.Module)
            .Include(x => x.ToNexusModsMods)
            .ThenInclude(x => x.NexusModsMod)
            .AsSplitQuery()
            .FirstAsync(x => x.NexusModsUserId == userId, ct);

        IQueryable<UserCrashReportView> DbQueryBase(Expression<Func<CrashReportEntity, bool>> predicate)
        {
            return _dbContextRead.CrashReports
                .Include(x => x.ToUsers)
                .ThenInclude(x => x.NexusModsUser)
                .Include(x => x.ModuleInfos)
                .ThenInclude(x => x.Module)
                .Include(x => x.ModuleInfos)
                .ThenInclude(x => x.NexusModsMod)
                .Include(x => x.ModuleInfos)
                .ThenInclude(x => x.Module)
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
                    ModuleIdToVersion = x.ModuleInfos.Select(y =>  new ModuleIdToVersionView { ModuleId = y.Module.ModuleId, Version = y.Version }).ToArray(),
                    TopInvolvedModuleId = x.ModuleInfos.Where(z => z.IsInvolved).Select(y => y.Module).Select(y => y.ModuleId).FirstOrDefault(),
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

        return new(await dbQuery.PaginatedAsync(page, pageSize, ct), items => items.Select(x => new CrashReportModel
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
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<PagingData<CrashReportModel>?>>> PaginatedAsync([FromBody] PaginatedQuery query, CancellationToken ct)
    {
        var (paginated, transform) = await PaginatedBaseAsync(query, ct);
        return APIPagingResponse(paginated, transform);
    }

    [HttpPost("PaginatedStreaming")]
    [Produces("application/x-ndjson-butr-paging")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<StreamingJsonActionResult> PaginatedStreamingAsync([FromBody] PaginatedQuery query, CancellationToken ct)
    {
        var (paginated, transform) = await PaginatedBaseAsync(query, ct);
        return APIPagingResponseStreaming(paginated, transform);
    }

    [HttpGet("Autocomplete")]
    [Produces("application/json")]
    public ActionResult<APIResponse<IQueryable<string>?>> Autocomplete([FromQuery] string modId)
    {
        return APIResponse(_dbContextRead.AutocompleteStartsWith<CrashReportToModuleMetadataEntity>(x => x.Module.ModuleId, modId));
    }

    [HttpPost("Update")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> UpdateAsync([FromBody] CrashReportModel updatedCrashReport)
    {
        var entityFactory = _dbContextWrite.CreateEntityFactory();
        await using var _ = _dbContextWrite.CreateSaveScope();

        var userId = HttpContext.GetUserId();
        var tenant = HttpContext.GetTenant();
        if (tenant is null) return BadRequest();

        var entity = new NexusModsUserToCrashReportEntity()
        {
            TenantId = tenant.Value,
            NexusModsUser = entityFactory.GetOrCreateNexusModsUser(userId),
            CrashReportId = updatedCrashReport.Id,
            Status = updatedCrashReport.Status,
            Comment = updatedCrashReport.Comment
        };
        _dbContextWrite.FutureUpsert(x => x.NexusModsUserToCrashReports, entity);
        return APIResponse("Updated successful!");
    }
}