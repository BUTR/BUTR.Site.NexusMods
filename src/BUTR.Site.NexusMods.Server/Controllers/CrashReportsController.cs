using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Services.Database;
using BUTR.Site.NexusMods.Shared.Helpers;

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
    public sealed class CrashReportsController : ControllerBase
    {
        public sealed record CrashReportsQuery(int Page, int PageSize);

        private readonly ILogger _logger;
        private readonly CrashReportsProvider _crashReports;
        private readonly UserCrashReportsProvider _userCrashReports;

        public CrashReportsController(ILogger<CrashReportsController> logger, CrashReportsProvider crashReports, UserCrashReportsProvider userCrashReports)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _crashReports = crashReports ?? throw new ArgumentNullException(nameof(crashReports));
            _userCrashReports = userCrashReports ?? throw new ArgumentNullException(nameof(userCrashReports));
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

        private async Task<ObjectResult> ForAdministrator(int page, int pageSize)
        {
            var (crashReportCount, crashReports) = await _crashReports.GetPaginatedAsync(-1, (page - 1) * pageSize, pageSize, CancellationToken.None);

            var metadata = new PagingMetadata
            {
                PageSize = pageSize,
                CurrentPage = page,
                TotalCount = crashReportCount,
                TotalPages = (int) Math.Floor((double) crashReportCount / (double) pageSize),
            };

            return StatusCode(StatusCodes.Status200OK, new PagingResponse<CrashReportModel>
            {
                Items = crashReports.Select(cr => new CrashReportModel(cr.Id, cr.GameVersion, cr.Exception, cr.CreatedAt, cr.Url, cr.InvolvedModIds)
                {
                    Status = cr.UserCrashReports.Any() ? cr.UserCrashReports.First().Status : CrashReportStatus.New,
                    Comment = cr.UserCrashReports.Any() ? cr.UserCrashReports.First().Comment : string.Empty
                }).ToAsyncEnumerable(),
                Metadata = metadata
            });
        }
        private async Task<ObjectResult> ForUser(int userId, int page, int pageSize)
        {
            var (crashReportCount, crashReports) = await _crashReports.GetPaginatedAsync(userId, (page - 1) * pageSize, pageSize, CancellationToken.None);

            var metadata = new PagingMetadata
            {
                PageSize = pageSize,
                CurrentPage = page,
                TotalCount = crashReportCount,
                TotalPages = (int) Math.Floor((double) crashReportCount / (double) pageSize),
            };

            return StatusCode(StatusCodes.Status200OK, new PagingResponse<CrashReportModel>
            {
                Items = crashReports.Select(cr => new CrashReportModel(cr.Id, cr.GameVersion, cr.Exception, cr.CreatedAt, cr.Url, cr.InvolvedModIds)
                {
                    Status = cr.UserCrashReports.Any() ? cr.UserCrashReports.First().Status : CrashReportStatus.New,
                    Comment = cr.UserCrashReports.Any() ? cr.UserCrashReports.First().Comment : string.Empty
                }).ToAsyncEnumerable(),
                Metadata = metadata
            });
        }


        [HttpPost("Update")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Update([FromBody] CrashReportModel updatedCrashReport)
        {
            var userId = HttpContext.GetUserId();

            if (await _userCrashReports.FindAsync(userId, updatedCrashReport.Id) is { } entry)
            {
                await _userCrashReports.UpsertAsync(entry with { Status = updatedCrashReport.Status, Comment = updatedCrashReport.Comment });
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Updated successful!"));
            }
            else
            {
                var crashReport = await _crashReports.FindAsync(updatedCrashReport.Id);
                //return StatusCode((int) HttpStatusCode.NotFound, new StandardResponse("Mod is not linked!"));

                await _userCrashReports.UpsertAsync(new UserCrashReportTableEntry
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