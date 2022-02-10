using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers
{
    [ApiController, Route("api/v1/[controller]"), AllowAnonymous]
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
        [Produces("text/html")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAll(string id)
        {
            var client = _httpClientFactory.CreateClient("CrashReporter");
            var data = await client.GetStringAsync($"{id}.html");

            return StatusCode(StatusCodes.Status200OK, data);
        }
    }
}