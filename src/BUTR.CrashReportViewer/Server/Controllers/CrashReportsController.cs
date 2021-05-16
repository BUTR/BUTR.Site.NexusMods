using BUTR.CrashReportViewer.Shared.Contexts;
using BUTR.CrashReportViewer.Shared.Helpers;
using BUTR.CrashReportViewer.Shared.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

using System;
using System.Collections.Generic;
using System.Linq;

namespace BUTR.CrashReportViewer.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CrashReportsController : ControllerBase
    {
        private readonly ILogger<CrashReportsController> _logger;
        private readonly NexusModsAPIClient _nexusModsAPIClient;
        private readonly ModsDbContext _modsDbContext;
        private readonly CrashReportsDbContext _crashReportsDbContext;

        public CrashReportsController(
            ILogger<CrashReportsController> logger,
            NexusModsAPIClient nexusModsAPIClient,
            ModsDbContext modsDbContext,
            CrashReportsDbContext crashReportsDbContext)
        {
            _logger = logger ?? throw  new ArgumentNullException(nameof(logger));
            _nexusModsAPIClient = nexusModsAPIClient ?? throw  new ArgumentNullException(nameof(nexusModsAPIClient));
            _modsDbContext = modsDbContext ?? throw  new ArgumentNullException(nameof(modsDbContext));
            _crashReportsDbContext = crashReportsDbContext ?? throw  new ArgumentNullException(nameof(crashReportsDbContext));
        }

        [HttpGet]
        public async IAsyncEnumerable<CrashReportModel> Get()
        {
            // We need the NexusMods API Key to confirm we are dealing with a legit User
            // and get his Id which we use to find his mods
            var apiKeyValues = Request.Headers.TryGetValue("apikey", out var val) ? val : StringValues.Empty;
            if (!apiKeyValues.Any())
            {
                yield break;
            }

            var apiKey = apiKeyValues.First();
            var validateResponse = await _nexusModsAPIClient.ValidateAPIKey(apiKey);
            if (validateResponse == null)
                yield break;

            var allowedMods = _modsDbContext.Mods
                .AsNoTracking()
                .AsEnumerable()
                .Where(m => m.UserIds.Contains(validateResponse.UserId))
                .ToList();

            var crashReports = _crashReportsDbContext.CrashReports
                .Include(cr => cr.UserCrashReports.Where(ucr => ucr.UserId == validateResponse.UserId))
                .AsNoTracking()
                .AsEnumerable()
                .Where(cr => cr.ModIds.Intersect(allowedMods.Select(m => m.ModId)).Any())
                .ToList();

            var crashReportModels = crashReports.Select(cr =>
            {
                var userData = cr.UserCrashReports.FirstOrDefault();
                return new CrashReportModel
                {
                    Id = cr.Id,
                    Date = cr.CreatedAt,
                    Comment = userData?.Comment ?? string.Empty,
                    Status = userData?.Status ?? CrashReportStatus.New
                };
            });
            foreach (var crashReportModel in crashReportModels)
            {
                yield return crashReportModel;
            }
        }
    }
}