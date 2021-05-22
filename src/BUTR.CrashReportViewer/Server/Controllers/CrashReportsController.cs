using BUTR.CrashReportViewer.Server.Contexts;
using BUTR.CrashReportViewer.Server.Helpers;
using BUTR.CrashReportViewer.Shared.Models;
using BUTR.CrashReportViewer.Shared.Models.API;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BUTR.CrashReportViewer.Server.Models.Contexts;

namespace BUTR.CrashReportViewer.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CrashReportsController : ControllerBase
    {
        public record CrashReportsQuery(int Page, int PageSize);


        private readonly ILogger _logger;
        private readonly NexusModsAPIClient _nexusModsAPIClient;
        private readonly MainDbContext _mainDbContext;

        public CrashReportsController(ILogger<CrashReportsController> logger, NexusModsAPIClient nexusModsAPIClient, MainDbContext mainDbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _nexusModsAPIClient = nexusModsAPIClient ?? throw new ArgumentNullException(nameof(nexusModsAPIClient));
            _mainDbContext = mainDbContext ?? throw new ArgumentNullException(nameof(mainDbContext));
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] CrashReportsQuery query)
        {
            var page = query.Page;
            var pageSize = Math.Max(Math.Min(query.PageSize, 50), 10);

            if (!HttpContext.User.HasClaim(c => c.Type == "nmapikey") || HttpContext.User.Claims.FirstOrDefault(c => c.Type == "nmapikey") is not { } apiKeyClaim)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Invalid Bearer!"));

            if (await _nexusModsAPIClient.ValidateAPIKey(apiKeyClaim.Value) is not { } validateResponse)
                return StatusCode((int) HttpStatusCode.Unauthorized, new StandardResponse("Invalid NexusMods API Key from Bearer!"));

            var userModIds = _mainDbContext.Mods
                .AsNoTracking()
                .Where(m => m.UserIds.Contains(validateResponse.UserId))
                .Select(m => m.ModId)
                .ToArray();

            var crashReportCount = _mainDbContext.CrashReports
                .AsNoTracking()
                .Count(cr => cr.ModIds.Any(crmi => userModIds.Contains(crmi)));

            var crashReports = _mainDbContext.CrashReports
                .Include(cr => cr.UserCrashReports.Where(ucr => ucr.UserId == validateResponse.UserId))
                .AsNoTracking()
                .OrderBy(cr => cr.CreatedAt)
                .Where(cr => cr.ModIds.Any(crmi => userModIds.Contains(crmi)))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(cr => new CrashReportModel(cr.Id, cr.Exception, cr.CreatedAt)
                {
                    Status = cr.UserCrashReports.Any() ? cr.UserCrashReports.First().Status : CrashReportStatus.New,
                    Comment = cr.UserCrashReports.Any() ? cr.UserCrashReports.First().Comment : string.Empty
                })
                .ToList();

            var metadata = new PagingMetadata
            {
                PageSize = pageSize,
                CurrentPage = page,
                TotalCount = crashReportCount,
                TotalPages = (int) Math.Floor((double) crashReportCount / (double) pageSize),
            };

            return StatusCode((int) HttpStatusCode.OK, new PagingResponse<CrashReportModel>
            {
                Items = crashReports,
                Metadata = metadata
            });
        }

        [HttpGet("Update")]
        public async Task<ActionResult> Update([FromBody] CrashReportModel updatedCrashReport)
        {
            if (!HttpContext.User.HasClaim(c => c.Type == "nmapikey") || HttpContext.User.Claims.FirstOrDefault(c => c.Type == "nmapikey") is not { } apiKeyClaim)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Invalid Bearer!"));

            if (await _nexusModsAPIClient.ValidateAPIKey(apiKeyClaim.Value) is not { } validateResponse)
                return StatusCode((int) HttpStatusCode.Unauthorized, new StandardResponse("Invalid NexusMods API Key from Bearer!"));

            if (await _mainDbContext.UserCrashReports.FindAsync(validateResponse.UserId, updatedCrashReport.Id) is { } userCrashReport)
            {
                userCrashReport.Status = updatedCrashReport.Status;
                userCrashReport.Comment = updatedCrashReport.Comment;
                await _mainDbContext.SaveChangesAsync();
                return StatusCode((int) HttpStatusCode.OK, new StandardResponse("Updated successful!"));
            }
            else
            {
                var crashReport = await _mainDbContext.CrashReports.FindAsync(updatedCrashReport.Id);
                //return StatusCode((int) HttpStatusCode.NotFound, new StandardResponse("Mod is not linked!"));

                await _mainDbContext.UserCrashReports.AddAsync(new UserCrashReportTable
                {
                    UserId = validateResponse.UserId,
                    CrashReport = crashReport,
                    Status = updatedCrashReport.Status,
                    Comment = updatedCrashReport.Comment,
                });
                await _mainDbContext.SaveChangesAsync();
                return StatusCode((int) HttpStatusCode.OK, new StandardResponse("Updated successful!"));
            }
        }
    }
}