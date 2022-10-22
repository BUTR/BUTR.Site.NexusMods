using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

using Npgsql;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers
{
    [ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
    public sealed class CrashReportsController : ControllerBase
    {
        public sealed record CrashReportsPaginated(uint Page, uint PageSize, List<Filtering>? Filters, List<Sorting>? Sotings);

        private sealed record UserCrashReportView
        {
            public Guid Id { get; init; } = default!;
            public int Version { get; init; } = default!;
            public string GameVersion { get; init; } = default!;
            public string Exception { get; init; } = default!;
            public DateTime CreatedAt { get; init; } = default!;
            public string[] ModIds { get; init; } = default!;
            public string[] InvolvedModIds { get; init; } = default!;
            public int[] ModNexusModsIds { get; init; } = default!;
            public string Url { get; init; } = default!;

            public int UserId { get; init; } = default!;
            public CrashReportStatus Status { get; init; } = default!;
            public string Comment { get; init; } = default!;
        }

        private readonly ILogger _logger;
        private readonly AppDbContext _dbContext;

        public CrashReportsController(ILogger<CrashReportsController> logger, AppDbContext dbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        [HttpPost("Paginated")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagingResponse<CrashReportModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Paginated([FromBody] CrashReportsPaginated query)
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
                    .GroupJoin(_dbContext.Set<UserCrashReportEntity>(), cre => cre.Id, ucr => ucr.CrashReport.Id, (cre, ucr) => new { cre, ucr })
                    .SelectMany(x => x.ucr.DefaultIfEmpty(), (cre, ucr) => new { cre.cre, ucr })
                    .Select(x => new UserCrashReportView
                    {
                        Id = x.cre.Id,
                        GameVersion = x.cre.GameVersion,
                        Exception = x.cre.Exception,
                        CreatedAt = x.cre.CreatedAt,
                        ModIds = x.cre.ModIds,
                        InvolvedModIds = x.cre.InvolvedModIds,
                        ModNexusModsIds = x.cre.ModNexusModsIds,
                        Url = x.cre.Url,
                        UserId = x.ucr != null ? x.ucr.UserId : -1,
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
                                   _dbContext.Set<ModNexusModsManualLinkEntity>().Any(y => _dbContext.Set<NexusModsModEntity>().Any(z => z.UserIds.Contains(userId) && z.NexusModsModId == y.NexusModsId) && x.ModIds.Contains(y.ModId)) ||
                                   _dbContext.Set<UserAllowedModsEntity>().Any(y => y.UserId == userId && x.ModIds.Any(z => y.AllowedModIds.Contains(z))));

            var paginated = await dbQuery.PaginatedAsync<UserCrashReportView>(page, pageSize, CancellationToken.None);

            return StatusCode(StatusCodes.Status200OK, new PagingResponse<CrashReportModel>
            {
                Items = paginated.Items.Select(x => new CrashReportModel
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
                }).ToAsyncEnumerable(),
                Metadata = paginated.Metadata
            });
        }
        [HttpGet("Autocomplete")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Paginated([FromQuery] string modId)
        {
            var mapping = _dbContext.Model.FindEntityType(typeof(CrashReportEntity));
            if (mapping is null || mapping.GetTableName() is not { } table)
                return StatusCode(StatusCodes.Status200OK, Array.Empty<string>());

            var schema = mapping.GetSchema();
            var tableName = mapping.GetSchemaQualifiedTableName();
            var storeObjectIdentifier = StoreObjectIdentifier.Table(table, schema);
            var modIdsName = mapping.GetProperty(nameof(CrashReportEntity.ModIds)).GetColumnName(storeObjectIdentifier);

            var valPram = new NpgsqlParameter<string>("val", modId);
            var values = await _dbContext.Set<CrashReportEntity>()
                .FromSqlRaw(@$"
WITH values AS (SELECT DISTINCT unnest({modIdsName}) as {modIdsName} FROM {tableName} ORDER BY {modIdsName})
SELECT
    array_agg({modIdsName}) as {modIdsName}
FROM
    values
WHERE
    {modIdsName} ILIKE @val || '%'", valPram)
                .AsNoTracking()
                .Select(x => x.ModIds)
                .ToArrayAsync();

            return StatusCode(StatusCodes.Status200OK, values.First());
        }

        [HttpPost("Update")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Update([FromBody] CrashReportModel updatedCrashReport)
        {
            var userId = HttpContext.GetUserId();

            UserCrashReportEntity? ApplyChanges(UserCrashReportEntity? existing) => existing switch
            {
                null => new UserCrashReportEntity
                {
                    CrashReport = new(updatedCrashReport.Id),
                    UserId = userId,
                    Status = updatedCrashReport.Status,
                    Comment = updatedCrashReport.Comment
                },
                var entity => entity with { Status = updatedCrashReport.Status, Comment = updatedCrashReport.Comment }
            };
            var set = _dbContext.Set<UserCrashReportEntity>().Include(x => x.CrashReport);
            if (await _dbContext.AddUpdateRemoveAndSaveAsync<UserCrashReportEntity>(set, x => x.UserId == userId && x.CrashReport.Id == updatedCrashReport.Id, ApplyChanges))
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Updated successful!"));

            return StatusCode(StatusCodes.Status200OK, new StandardResponse("Failed to update!"));
        }
    }
}