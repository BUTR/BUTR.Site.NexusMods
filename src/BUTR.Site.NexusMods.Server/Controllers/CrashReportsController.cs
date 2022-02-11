using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Services.Database;
using BUTR.Site.NexusMods.Shared.Helpers;
using BUTR.Site.NexusMods.Shared.Models;
using BUTR.Site.NexusMods.Shared.Models.API;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers
{
    [ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
    public class CrashReportsController : ControllerBase
    {
        public record CrashReportsQuery(int Page, int PageSize);

        private readonly ILogger _logger;
        private readonly CrashReportsProvider _crashReportsProvider;
        private readonly UserCrashReportsProvider _sqlHelperUserCrashReports;

        public CrashReportsController(ILogger<CrashReportsController> logger, CrashReportsProvider sqlHelperCrashReports, UserCrashReportsProvider sqlHelperUserCrashReports)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _crashReportsProvider = sqlHelperCrashReports ?? throw new ArgumentNullException(nameof(sqlHelperCrashReports));
            _sqlHelperUserCrashReports = sqlHelperUserCrashReports ?? throw new ArgumentNullException(nameof(sqlHelperUserCrashReports));
        }

        [HttpGet("")]
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

        private async Task<ObjectResult> ForAdministrator(int page, int pageSize)
        {
            var (crashReportCount, crashReports) = await _crashReportsProvider.GetPaginatedAsync(-1, (page - 1) * pageSize, pageSize, CancellationToken.None);

            var metadata = new PagingMetadata
            {
                PageSize = pageSize,
                CurrentPage = page,
                TotalCount = crashReportCount,
                TotalPages = (int) Math.Floor((double) crashReportCount / (double) pageSize),
            };

            return StatusCode(StatusCodes.Status200OK, new PagingResponse<CrashReportModel>
            {
                Items = crashReports.Select(cr => new CrashReportModel(cr.Id, cr.Exception, cr.CreatedAt, cr.Url)
                {
                    Status = cr.UserCrashReports.Any() ? cr.UserCrashReports.First().Status : CrashReportStatus.New,
                    Comment = cr.UserCrashReports.Any() ? cr.UserCrashReports.First().Comment : string.Empty
                }).ToAsyncEnumerable(),
                Metadata = metadata
            });
        }
        private async Task<ObjectResult> ForUser(int userId, int page, int pageSize)
        {
            var (crashReportCount, crashReports) = await _crashReportsProvider.GetPaginatedAsync(userId, (page - 1) * pageSize, pageSize, CancellationToken.None);

            var metadata = new PagingMetadata
            {
                PageSize = pageSize,
                CurrentPage = page,
                TotalCount = crashReportCount,
                TotalPages = (int) Math.Floor((double) crashReportCount / (double) pageSize),
            };

            return StatusCode(StatusCodes.Status200OK, new PagingResponse<CrashReportModel>
            {
                Items = crashReports.Select(cr => new CrashReportModel(cr.Id, cr.Exception, cr.CreatedAt, cr.Url)
                {
                    Status = cr.UserCrashReports.Any() ? cr.UserCrashReports.First().Status : CrashReportStatus.New,
                    Comment = cr.UserCrashReports.Any() ? cr.UserCrashReports.First().Comment : string.Empty
                }).ToAsyncEnumerable(),
                Metadata = metadata
            });
        }


        [HttpPost("")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Update([FromBody] CrashReportModel updatedCrashReport)
        {
            if (User.IsInRole(ApplicationRoles.Administrator) || User.IsInRole(ApplicationRoles.Moderator))
                return await UpdateForAdministrator(updatedCrashReport);

            return await UpdateForUser(HttpContext.GetUserId(), updatedCrashReport);
        }

        private async Task<ObjectResult> UpdateForAdministrator(CrashReportModel updatedCrashReport)
        {
            if (await _sqlHelperUserCrashReports.FindAsync(-1, updatedCrashReport.Id) is { } userCrashReport)
            {
                userCrashReport.Status = updatedCrashReport.Status;
                userCrashReport.Comment = updatedCrashReport.Comment;
                await _sqlHelperUserCrashReports.UpsertAsync(userCrashReport);
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Updated successful!"));
            }
            else
            {
                var crashReport = await _crashReportsProvider.FindAsync(updatedCrashReport.Id);
                //return StatusCode((int) HttpStatusCode.NotFound, new StandardResponse("Mod is not linked!"));

                await _sqlHelperUserCrashReports.UpsertAsync(new UserCrashReportTableEntry
                {
                    UserId = -1,
                    CrashReport = crashReport,
                    Status = updatedCrashReport.Status,
                    Comment = updatedCrashReport.Comment,
                });
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Updated successful!"));
            }
        }
        private async Task<ObjectResult> UpdateForUser(int userId, CrashReportModel updatedCrashReport)
        {
            if (await _sqlHelperUserCrashReports.FindAsync(userId, updatedCrashReport.Id) is { } userCrashReport)
            {
                userCrashReport.Status = updatedCrashReport.Status;
                userCrashReport.Comment = updatedCrashReport.Comment;
                await _sqlHelperUserCrashReports.UpsertAsync(userCrashReport);
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Updated successful!"));
            }
            else
            {
                var crashReport = await _crashReportsProvider.FindAsync(updatedCrashReport.Id);
                //return StatusCode((int) HttpStatusCode.NotFound, new StandardResponse("Mod is not linked!"));

                await _sqlHelperUserCrashReports.UpsertAsync(new UserCrashReportTableEntry
                {
                    UserId = userId,
                    CrashReport = crashReport,
                    Status = updatedCrashReport.Status,
                    Comment = updatedCrashReport.Comment,
                });
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Updated successful!"));
            }
        }
    }
}