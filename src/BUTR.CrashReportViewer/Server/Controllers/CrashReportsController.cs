using BUTR.CrashReportViewer.Shared.Contexts;
using BUTR.CrashReportViewer.Shared.Helpers;
using BUTR.CrashReportViewer.Shared.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BUTR.CrashReportViewer.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CrashReportsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly NexusModsAPIClient _nexusModsAPIClient;
        private readonly MainDbContext _mainDbContext;

        public CrashReportsController(ILogger<CrashReportsController> logger, NexusModsAPIClient nexusModsAPIClient, MainDbContext mainDbContext)
        {
            _logger = logger ?? throw  new ArgumentNullException(nameof(logger));
            _nexusModsAPIClient = nexusModsAPIClient ?? throw  new ArgumentNullException(nameof(nexusModsAPIClient));
            _mainDbContext = mainDbContext ?? throw  new ArgumentNullException(nameof(mainDbContext));
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromHeader] string? apiKey)
        {
            if (apiKey is null)
                return StatusCode((int) HttpStatusCode.BadRequest, "API Key not found!");

            var validateResponse = await _nexusModsAPIClient.ValidateAPIKey(apiKey);
            if (validateResponse == null)
                return StatusCode((int) HttpStatusCode.Unauthorized, "Invalid API Key!");

            var userMods = _mainDbContext.Mods
                .AsNoTracking()
                .AsEnumerable()
                .Where(m => m.UserIds.Contains(validateResponse.UserId))
                .ToArray();

            var crashReports = _mainDbContext.CrashReports
                .Include(cr => cr.UserCrashReports.Where(ucr => ucr.UserId == validateResponse.UserId))
                .AsNoTracking()
                .AsEnumerable()
                .Where(cr => cr.ModIds.Intersect(userMods.Select(m => m.ModId)).Any());

            var crashReportModels = crashReports
                .Select(cr =>
                {
                    var userData = cr.UserCrashReports.FirstOrDefault();
                    return new CrashReportModel
                    {
                        Id = cr.Id,
                        Date = cr.CreatedAt,
                        Comment = userData?.Comment ?? string.Empty,
                        Status = userData?.Status ?? CrashReportStatus.New
                    };
                })
                .OrderBy(cr => cr.Date);
            return StatusCode((int) HttpStatusCode.OK, crashReportModels);
        }
    }
}