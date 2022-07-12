using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers
{
    [ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
    public sealed class CrashReportsController : ControllerBase
    {
        public sealed record CrashReportsQuery(uint Page, uint PageSize);

        private readonly ILogger _logger;
        private readonly AppDbContext _dbContext;

        public CrashReportsController(ILogger<CrashReportsController> logger, AppDbContext dbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        [HttpGet("Paginated")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagingResponse<CrashReportModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Paginated([FromQuery] CrashReportsQuery query)
        {
            var page = query.Page;
            var pageSize = Math.Max(Math.Min(query.PageSize, 50), 10);

            if (User.IsInRole(ApplicationRoles.Administrator) || User.IsInRole(ApplicationRoles.Moderator))
                return await ForAdministrator(page, pageSize);

            return await ForUser(HttpContext.GetUserId(), page, pageSize);
        }

        private async Task<ObjectResult> ForAdministrator(uint page, uint pageSize)
        {
            var dbQuery = _dbContext.Set<CrashReportEntity>()
                .OrderByDescending(x => x.CreatedAt)
                .Include(x => x.UserCrashReports);

            var paginated = await dbQuery.PaginatedAsync<CrashReportEntity>(page, pageSize, CancellationToken.None);

            return StatusCode(StatusCodes.Status200OK, new PagingResponse<CrashReportModel>
            {
                Items = paginated.Items.Select(cr => new CrashReportModel(cr.Id, cr.GameVersion, cr.Exception, cr.CreatedAt, cr.Url, cr.InvolvedModIds.ToImmutableArray())
                {
                    Status = cr.UserCrashReports.Any() ? cr.UserCrashReports.First().Status : CrashReportStatus.New,
                    Comment = cr.UserCrashReports.Any() ? cr.UserCrashReports.First().Comment : string.Empty
                }).ToAsyncEnumerable(),
                Metadata = paginated.Metadata
            });
        }
        private async Task<ObjectResult> ForUser(int userId, uint page, uint pageSize)
        {
            var dbQuery =
                (from cre in _dbContext.Set<CrashReportEntity>()
                 from nmm in _dbContext.Set<NexusModsModEntity>().Where(x => cre.ModNexusModsIds.Contains(x.ModId)).DefaultIfEmpty()
                 from umi in _dbContext.Set<UserAllowedModsEntity>().Where(x => cre.ModIds.Any(y => x.UserId == userId && x.AllowedModIds.Contains(y))).DefaultIfEmpty()
                 from mnm in _dbContext.Set<ModNexusModsManualLinkEntity>().Where(x => cre.ModIds.Contains(x.ModId)).DefaultIfEmpty()
                 where nmm != null || umi != null || mnm != null
                 select cre)
                .Distinct()
                .OrderByDescending(x => x.CreatedAt)
                .Include(x => x.UserCrashReports);

            var paginated = await dbQuery.PaginatedAsync<CrashReportEntity>(page, pageSize, CancellationToken.None);

            return StatusCode(StatusCodes.Status200OK, new PagingResponse<CrashReportModel>
            {
                Items = paginated.Items.Select(cr => new CrashReportModel(cr.Id, cr.GameVersion, cr.Exception, cr.CreatedAt, cr.Url, cr.InvolvedModIds.ToImmutableArray())
                {
                    Status = cr.UserCrashReports.Any() ? cr.UserCrashReports.First().Status : CrashReportStatus.New,
                    Comment = cr.UserCrashReports.Any() ? cr.UserCrashReports.First().Comment : string.Empty
                }).ToAsyncEnumerable(),
                Metadata = paginated.Metadata
            });
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
                null => null,
                var entity => entity with { Status = updatedCrashReport.Status, Comment = updatedCrashReport.Comment }
            };
            var set = _dbContext.Set<UserCrashReportEntity>().Include(x => x.CrashReport);
            if (await _dbContext.AddUpdateRemoveAndSaveAsync<UserCrashReportEntity>(set, x => x.UserId == userId && x.CrashReport.Id == updatedCrashReport.Id, ApplyChanges))
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Updated successful!"));

            return StatusCode(StatusCodes.Status200OK, new StandardResponse("Failed to update!"));
        }
    }
}