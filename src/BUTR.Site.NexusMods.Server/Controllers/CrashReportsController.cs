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
    private sealed record UserCrashReportView
    {
        public required Guid Id { get; init; }
        public required int Version { get; init; }
        public required string GameVersion { get; init; }
        public required string Exception { get; init; }
        public required DateTime CreatedAt { get; init; }
        public required string[] ModIds { get; init; }
        public required Dictionary<string, string> ModIdToVersion { get; init; }
        public required string[] InvolvedModIds { get; init; }
        public required int[] ModNexusModsIds { get; init; }
        public required string Url { get; init; }

        public required int UserId { get; init; }
        public required CrashReportStatus Status { get; init; }
        public required string Comment { get; init; }
    }

    private readonly ILogger _logger;
    private readonly AppDbContext _dbContext;

    public CrashReportsController(ILogger<CrashReportsController> logger, AppDbContext dbContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    private async Task<BasePaginated<UserCrashReportView, CrashReportModel>> PaginatedBaseAsync(PaginatedQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var pageSize = Math.Max(Math.Min(query.PageSize, 50), 5);
        var filters = query.Filters ?? Enumerable.Empty<Filtering>();
        var sortings = query.Sotings is null || query.Sotings.Count == 0
            ? new List<Sorting> { new() { Property = nameof(CrashReportEntity.CreatedAt), Type = SortingType.Descending } }
            : query.Sotings;

        IQueryable<UserCrashReportView> DbQueryBase(Expression<Func<CrashReportEntity, bool>> predicate)
        {
            return _dbContext.Set<CrashReportEntity>()
                .Where(predicate)
                .GroupJoin(_dbContext.Set<NexusModsUserCrashReportEntity>(), cre => cre.Id, ucr => ucr.CrashReport.Id, (cre, ucr) => new { cre, ucr })
                .SelectMany(x => x.ucr.DefaultIfEmpty(), (cre, ucr) => new { cre.cre, ucr })
                .Select(x => new UserCrashReportView
                {
                    Id = x.cre.Id,
                    Version = x.cre.Version,
                    GameVersion = x.cre.GameVersion,
                    Exception = x.cre.Exception,
                    CreatedAt = x.cre.CreatedAt,
                    ModIds = x.cre.ModIds,
                    ModIdToVersion = x.cre.ModIdToVersion,
                    InvolvedModIds = x.cre.InvolvedModIds,
                    ModNexusModsIds = x.cre.ModNexusModsIds,
                    Url = x.cre.Url,
                    UserId = x.ucr != null ? x.ucr.NexusModsUserId : -1,
                    Status = x.ucr != null ? x.ucr.Status : CrashReportStatus.New,
                    Comment = x.ucr != null ? x.ucr.Comment : string.Empty,
                })
                .WithFilter(filters)
                .WithSort(sortings);
        }

        var userId = HttpContext.GetUserId();
        var dbQuery = User.IsInRole(ApplicationRoles.Administrator) || User.IsInRole(ApplicationRoles.Moderator)
            ? DbQueryBase(x => true)
            : DbQueryBase(x => _dbContext.Set<NexusModsModEntity>().Any(y => y.UserIds.Contains(userId) && x.ModNexusModsIds.Contains(y.NexusModsModId)) ||
                               _dbContext.Set<NexusModsModManualLinkedNexusModsUsersEntity>()
                                   .Any(y => x.ModNexusModsIds.Contains(y.NexusModsModId) && y.AllowedNexusModsUserIds.Contains(userId)) ||
                               _dbContext.Set<NexusModsModManualLinkedModuleIdEntity>().Any(y =>
                                   _dbContext.Set<NexusModsModEntity>().Any(z => z.UserIds.Contains(userId) && z.NexusModsModId == y.NexusModsModId) && x.ModIds.Contains(y.ModuleId)) ||
                               _dbContext.Set<NexusModsUserAllowedModuleIdsEntity>().Any(y => y.NexusModsUserId == userId && x.ModIds.Any(z => y.AllowedModuleIds.Contains(z))));

        return new(await dbQuery.Prepare().PaginatedAsync(page, pageSize, ct), items => items.Select(x => new CrashReportModel
        {
            Id = x.Id,
            Version = x.Version,
            GameVersion = x.GameVersion,
            Exception = x.Exception,
            Date = x.CreatedAt,
            Url = x.Url,
            InvolvedModules = x.InvolvedModIds.ToImmutableArray(),
            Status = x.Status,
            Comment = x.Comment
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
        return APIResponse(_dbContext.AutocompleteStartsWith<CrashReportEntity, string[]>(x => x.ModIds, modId).Prepare());
    }

    [HttpPost("Update")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> UpdateAsync([FromBody] CrashReportModel updatedCrashReport)
    {
        var userId = HttpContext.GetUserId();

        NexusModsUserCrashReportEntity? ApplyChanges(NexusModsUserCrashReportEntity? existing) => existing switch
        {
            null => new NexusModsUserCrashReportEntity
            {
                CrashReport = new(updatedCrashReport.Id),
                NexusModsUserId = userId,
                Status = updatedCrashReport.Status,
                Comment = updatedCrashReport.Comment
            },
            var entity => entity with { Status = updatedCrashReport.Status, Comment = updatedCrashReport.Comment }
        };
        var set = _dbContext.Set<NexusModsUserCrashReportEntity>().Include(x => x.CrashReport);
        if (await _dbContext.AddUpdateRemoveAndSaveAsync(set, x => x.NexusModsUserId == userId && x.CrashReport.Id == updatedCrashReport.Id, ApplyChanges))
            return APIResponse("Updated successful!");

        return APIResponseError<string>("Failed to update!");
    }
}