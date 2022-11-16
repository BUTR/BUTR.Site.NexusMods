using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

using Npgsql;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers
{
    [ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
    public class ExposedModsController : ControllerBase
    {
        public sealed record ExposedModsQuery(uint Page, uint PageSize, List<Filtering>? Filters, List<Sorting>? Sotings);

        private readonly ILogger _logger;
        private readonly AppDbContext _dbContext;

        public ExposedModsController(ILogger<ExposedModsController> logger, AppDbContext dbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        [HttpPost("Paginated")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagingResponse<ExposedModModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Paginated([FromBody] ExposedModsQuery query, CancellationToken ct)
        {
            var page = query.Page;
            var pageSize = Math.Max(Math.Min(query.PageSize, 100), 5);
            var filters = query.Filters ?? Enumerable.Empty<Filtering>();
            var sortings = query.Sotings is null || query.Sotings.Count == 0
                ? new List<Sorting> { new() { Property = nameof(NexusModsExposedModsEntity.NexusModsModId), Type = SortingType.Ascending } }
                : query.Sotings;

            var paginated = await _dbContext.Set<NexusModsExposedModsEntity>()
                .WithFilter(filters)
                .WithSort(sortings)
                .PaginatedAsync(page, pageSize, ct);

            return StatusCode(StatusCodes.Status200OK, new PagingResponse<ExposedModModel>
            {
                Items = paginated.Items.Select(x => new ExposedModModel(x.NexusModsModId, x.ModIds, x.LastCheckedDate)).ToAsyncEnumerable(),
                Metadata = paginated.Metadata
            });
        }

        [HttpGet("Autocomplete")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Autocomplete([FromQuery] string authorName)
        {
            var mapping = _dbContext.Model.FindEntityType(typeof(NexusModsExposedModsEntity));
            if (mapping is null || mapping.GetTableName() is not { } table)
                return StatusCode(StatusCodes.Status200OK, Array.Empty<string>());

            var schema = mapping.GetSchema();
            var tableName = mapping.GetSchemaQualifiedTableName();
            var storeObjectIdentifier = StoreObjectIdentifier.Table(table, schema);
            var modIdsName = mapping.GetProperty(nameof(NexusModsExposedModsEntity.ModIds)).GetColumnName(storeObjectIdentifier);

            var valPram = new NpgsqlParameter<string>("val", authorName);
            var values = await _dbContext.Set<NexusModsExposedModsEntity>()
                .FromSqlRaw(@$"
WITH values AS (SELECT DISTINCT unnest({modIdsName}) as {modIdsName} FROM {tableName} ORDER BY {modIdsName})
SELECT
    array_agg({modIdsName}) as {modIdsName}
FROM
    values
WHERE
    {modIdsName} ILIKE @val || '%'", valPram)
                .AsNoTracking()
                .Select(x => x.ModIds)
                .ToArrayAsync();

            return StatusCode(StatusCodes.Status200OK, values.First());
        }
    }
}