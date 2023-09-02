﻿using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
public class NexusModsArticleController : ControllerExtended
{
    public record NexusModsArticleModel(int NexusModsArticleId, string Title, int NexusModsUserId, string Author, DateTimeOffset CreateDate);


    private readonly ILogger _logger;
    private readonly IAppDbContextRead _dbContextRead;

    public NexusModsArticleController(ILogger<NexusModsArticleController> logger, IAppDbContextRead dbContextRead)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContextRead = dbContextRead ?? throw new ArgumentNullException(nameof(dbContextRead));
    }

    [HttpPost("Paginated")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<PagingData<NexusModsArticleModel>?>>> PaginatedAsync([FromBody] PaginatedQuery query, CancellationToken ct)
    {
        var paginated = await _dbContextRead.NexusModsArticles
            .Include(x => x.NexusModsUser)
            .ThenInclude(x => x.Name)
            .Select(x => new
            {
                NexusModsArticleId = x.NexusModsArticleId,
                Title = x.Title,
                NexusModsUserId = x.NexusModsUser.NexusModsUserId,
                Author = x.NexusModsUser.Name == null ? string.Empty : x.NexusModsUser.Name.Name,
                CreateDate = x.CreateDate,
            })
            .PaginatedAsync(query, 100, new() { Property = nameof(NexusModsArticleEntity.NexusModsArticleId), Type = SortingType.Ascending }, ct);

        return APIPagingResponse(paginated, items => items.Select(x => new NexusModsArticleModel(x.NexusModsArticleId, x.Title, x.NexusModsUserId, x.Author, x.CreateDate)));
    }

    [HttpGet("Autocomplete")]
    [Produces("application/json")]
    public ActionResult<APIResponse<IQueryable<string>?>> Autocomplete([FromQuery] string authorName)
    {
        var moduleIds = _dbContextRead.NexusModsArticles
            .Include(x => x.NexusModsUser)
            .ThenInclude(x => x.Name)
            .Select(x => x.NexusModsUser)
            .Select(x => x.Name!)
            .Where(x => EF.Functions.ILike(x.Name, $"{authorName}%"))
            .Select(x => x.Name)
            .Distinct();

        return APIResponse(moduleIds);

        //return APIResponse(_dbContextRead.AutocompleteStartsWith<NexusModsArticleEntity>(x => x.NexusModsUser.Name.Name, authorName));
    }
}