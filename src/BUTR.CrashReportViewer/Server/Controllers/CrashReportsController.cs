using BUTR.CrashReportViewer.Server.Helpers;
using BUTR.CrashReportViewer.Server.Models.Database;
using BUTR.CrashReportViewer.Shared.Models;
using BUTR.NexusMods.Server.Core.Helpers;
using BUTR.NexusMods.Shared.Helpers;
using BUTR.NexusMods.Shared.Models.API;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.CrashReportViewer.Server.Controllers
{
    [ApiController, Route("[controller]"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CrashReportsController : ControllerBase
    {
        public record CrashReportsQuery(int Page, int PageSize);

        private readonly ILogger _logger;
        private readonly NexusModsAPIClient _nexusModsAPIClient;
        private readonly SqlHelperCrashReports _sqlHelperCrashReports;
        private readonly SqlHelperUserCrashReports _sqlHelperUserCrashReports;

        public CrashReportsController(
            ILogger<CrashReportsController> logger,
            NexusModsAPIClient nexusModsAPIClient,
            SqlHelperCrashReports sqlHelperCrashReports,
            SqlHelperUserCrashReports sqlHelperUserCrashReports)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _nexusModsAPIClient = nexusModsAPIClient ?? throw new ArgumentNullException(nameof(nexusModsAPIClient));
            _sqlHelperCrashReports = sqlHelperCrashReports ?? throw new ArgumentNullException(nameof(sqlHelperCrashReports));
            _sqlHelperUserCrashReports = sqlHelperUserCrashReports ?? throw new ArgumentNullException(nameof(sqlHelperUserCrashReports));
        }

        [HttpGet("")]
        public async Task<ActionResult> GetAll([FromQuery] CrashReportsQuery query)
        {
            var page = query.Page;
            var pageSize = Math.Max(Math.Min(query.PageSize, 50), 10);

            if (User.IsInRole(ApplicationRoles.Administrator) || User.IsInRole(ApplicationRoles.Moderator))
                return await ForAdministrator(page, pageSize);

            if (!HttpContext.User.HasClaim(c => c.Type == "nmapikey") || HttpContext.User.Claims.FirstOrDefault(c => c.Type == "nmapikey") is not { } apiKeyClaim)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Invalid Bearer!"));

            if (await _nexusModsAPIClient.ValidateAPIKey(apiKeyClaim.Value) is not { } validateResponse)
                return StatusCode((int) HttpStatusCode.Unauthorized, new StandardResponse("Invalid NexusMods API Key from Bearer!"));

            return await ForUser(validateResponse.UserId, page, pageSize);
        }

        private async Task<ObjectResult> ForAdministrator(int page, int pageSize)
        {
            var (crashReportCount, crashReports) = await _sqlHelperCrashReports.GetAsync(-1, (page - 1) * pageSize, pageSize, CancellationToken.None);

            var metadata = new PagingMetadata
            {
                PageSize = pageSize,
                CurrentPage = page,
                TotalCount = crashReportCount,
                TotalPages = (int) Math.Floor((double) crashReportCount / (double) pageSize),
            };

            return StatusCode((int) HttpStatusCode.OK, new PagingResponse<CrashReportModel>
            {
                Items = crashReports.Select(cr => new CrashReportModel(cr.Id, cr.Exception, cr.CreatedAt, cr.Url)
                {
                    Status = cr.UserCrashReports.Any() ? cr.UserCrashReports.First().Status : CrashReportStatus.New,
                    Comment = cr.UserCrashReports.Any() ? cr.UserCrashReports.First().Comment : string.Empty
                }),
                Metadata = metadata
            });
        }
        private async Task<ObjectResult> ForUser(int userId, int page, int pageSize)
        {
            var (crashReportCount, crashReports) = await _sqlHelperCrashReports.GetAsync(userId, (page - 1) * pageSize, pageSize, CancellationToken.None);

            var metadata = new PagingMetadata
            {
                PageSize = pageSize,
                CurrentPage = page,
                TotalCount = crashReportCount,
                TotalPages = (int) Math.Floor((double) crashReportCount / (double) pageSize),
            };

            return StatusCode((int) HttpStatusCode.OK, new PagingResponse<CrashReportModel>
            {
                Items = crashReports.Select(cr => new CrashReportModel(cr.Id, cr.Exception, cr.CreatedAt, cr.Url)
                {
                    Status = cr.UserCrashReports.Any() ? cr.UserCrashReports.First().Status : CrashReportStatus.New,
                    Comment = cr.UserCrashReports.Any() ? cr.UserCrashReports.First().Comment : string.Empty
                }),
                Metadata = metadata
            });
        }

        [HttpPost("Update")]
        public async Task<ActionResult> Update([FromBody] CrashReportModel updatedCrashReport)
        {
            if (User.IsInRole(ApplicationRoles.Administrator) || User.IsInRole(ApplicationRoles.Moderator))
                return await UpdateForAdministrator(updatedCrashReport);

            if (!HttpContext.User.HasClaim(c => c.Type == "nmapikey") || HttpContext.User.Claims.FirstOrDefault(c => c.Type == "nmapikey") is not { } apiKeyClaim)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Invalid Bearer!"));

            if (await _nexusModsAPIClient.ValidateAPIKey(apiKeyClaim.Value) is not { } validateResponse)
                return StatusCode((int) HttpStatusCode.Unauthorized, new StandardResponse("Invalid NexusMods API Key from Bearer!"));

            return await UpdateForUser(validateResponse.UserId, updatedCrashReport);
        }

        private async Task<ObjectResult> UpdateForAdministrator(CrashReportModel updatedCrashReport)
        {
            if (await _sqlHelperUserCrashReports.FindAsync(-1, updatedCrashReport.Id) is { } userCrashReport)
            {
                userCrashReport.Status = updatedCrashReport.Status;
                userCrashReport.Comment = updatedCrashReport.Comment;
                await _sqlHelperUserCrashReports.UpsertAsync(userCrashReport);
                return StatusCode((int) HttpStatusCode.OK, new StandardResponse("Updated successful!"));
            }
            else
            {
                var crashReport = await _sqlHelperCrashReports.FindAsync(updatedCrashReport.Id);
                //return StatusCode((int) HttpStatusCode.NotFound, new StandardResponse("Mod is not linked!"));

                await _sqlHelperUserCrashReports.UpsertAsync(new UserCrashReportTableEntry
                {
                    UserId = -1,
                    CrashReport = crashReport,
                    Status = updatedCrashReport.Status,
                    Comment = updatedCrashReport.Comment,
                });
                return StatusCode((int) HttpStatusCode.OK, new StandardResponse("Updated successful!"));
            }
        }
        private async Task<ObjectResult> UpdateForUser(int userId, CrashReportModel updatedCrashReport)
        {
            if (await _sqlHelperUserCrashReports.FindAsync(userId, updatedCrashReport.Id) is { } userCrashReport)
            {
                userCrashReport.Status = updatedCrashReport.Status;
                userCrashReport.Comment = updatedCrashReport.Comment;
                await _sqlHelperUserCrashReports.UpsertAsync(userCrashReport);
                return StatusCode((int) HttpStatusCode.OK, new StandardResponse("Updated successful!"));
            }
            else
            {
                var crashReport = await _sqlHelperCrashReports.FindAsync(updatedCrashReport.Id);
                //return StatusCode((int) HttpStatusCode.NotFound, new StandardResponse("Mod is not linked!"));

                await _sqlHelperUserCrashReports.UpsertAsync(new UserCrashReportTableEntry
                {
                    UserId = userId,
                    CrashReport = crashReport,
                    Status = updatedCrashReport.Status,
                    Comment = updatedCrashReport.Comment,
                });
                return StatusCode((int) HttpStatusCode.OK, new StandardResponse("Updated successful!"));
            }
        }
    }
}