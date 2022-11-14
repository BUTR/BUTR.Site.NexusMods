﻿using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Threading;

namespace BUTR.Site.NexusMods.Server.Controllers
{
    [ApiController, Route("api/v1/[controller]")]
    public sealed class GamePublicApiDiffController : ControllerBase
    {
        private const string basePath = "/public-api-diff";
        //private const string basePath = "D:\\Git\\BUTR.Site.NexusMods\\diff\\";

        private readonly ILogger _logger;
        private readonly DiffProvider _diffProvider;

        public GamePublicApiDiffController(ILogger<GamePublicApiDiffController> logger, DiffProvider diffProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _diffProvider = diffProvider ?? throw new ArgumentNullException(nameof(diffProvider));
        }

        [HttpGet("List")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        public ActionResult List() => Ok(_diffProvider.List(basePath));

        [HttpGet("TreeFlat")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        public ActionResult TreeFlat(string entry) => Ok(_diffProvider.TreeFlat(basePath, entry));

        [HttpGet("Get")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        public ActionResult Get(string path, CancellationToken ct) => Ok(_diffProvider.Get(basePath, path, ct));

        [HttpPost("Search")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        public ActionResult Search(TextSearchFiltering[] filters, CancellationToken ct) => Ok(_diffProvider.Search(basePath, filters, ct));
    }
}