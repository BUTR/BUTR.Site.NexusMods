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

namespace BUTR.CrashReportViewer.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CrashReportsController : ControllerBase
    {
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
        public async Task<ActionResult> Get()
        {
            if (!HttpContext.User.HasClaim(c => c.Type == "nmapikey") || HttpContext.User.Claims.FirstOrDefault(c => c.Type == "nmapikey") is not { } apiKeyClaim)
                return StatusCode((int) HttpStatusCode.BadRequest, new StandardResponse("Invalid Bearer!"));

            if (await _nexusModsAPIClient.ValidateAPIKey(apiKeyClaim.Value) is not { } validateResponse)
                return StatusCode((int) HttpStatusCode.Unauthorized, new StandardResponse("Invalid NexusMods API Key from Bearer!"));

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
                    return new CrashReportModel(cr.Id, cr.Exception, cr.CreatedAt)
                    {
                        Status = userData?.Status ?? CrashReportStatus.New,
                        Comment = userData?.Comment ?? string.Empty
                    };
                })
                .OrderBy(cr => cr.Date);
            return StatusCode((int) HttpStatusCode.OK, crashReportModels);
        }
    }
}