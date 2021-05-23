using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BUTR.CrashReportViewer.Server.Controllers
{
    [ApiController, Route("[controller]"), AllowAnonymous]
    public class ReportsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ReportsController(ILogger<ReportsController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        [HttpGet("{id}.html")]
        public async Task<ActionResult> GetAll(string id)
        {
            var client = _httpClientFactory.CreateClient("CrashReporter");
            var data = await client.GetStringAsync($"{id}.html");

            return StatusCode((int) HttpStatusCode.OK, data);
        }
    }
}