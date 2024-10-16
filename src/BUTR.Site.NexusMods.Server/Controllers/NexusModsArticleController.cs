using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Repositories;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.BindingSources;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), ButrNexusModsAuthorization, TenantRequired]
public class NexusModsArticleController : ApiControllerBase
{
    public sealed record NexusModsArticleModel
    {
        public NexusModsArticleId NexusModsArticleId { get; init; }
        public string Title { get; init; } = default!;
        public NexusModsUserId NexusModsUserId { get; init; }
        public NexusModsUserName Author { get; init; }
        public DateTimeOffset CreateDate { get; init; }
    }


    private readonly ILogger _logger;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public NexusModsArticleController(ILogger<NexusModsArticleController> logger, IUnitOfWorkFactory unitOfWorkFactory)
    {
        _logger = logger;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    [HttpPost("Paginated")]
    public async Task<ApiResult<PagingData<NexusModsArticleModel>?>> GetPaginatedAsync([FromBody, Required] PaginatedQuery query, CancellationToken ct)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var paginated = await unitOfRead.NexusModsArticles.PaginatedAsync(x =>
            new NexusModsArticleModel
            {
                NexusModsArticleId = x.NexusModsArticleId,
                Title = x.Title,
                NexusModsUserId = x.NexusModsUserId,
                Author = x.NexusModsUser.Name != null ? x.NexusModsUser.Name.Name : NexusModsUserName.Empty,
                CreateDate = x.CreateDate
            },
            query, 100, new() { Property = nameof(NexusModsArticleModel.NexusModsArticleId), Type = SortingType.Ascending }, ct);

        return ApiPagingResult(paginated);
    }

    [HttpGet("Autocompletes/AuthorNames")]
    public async Task<ApiResult<IList<string>?>> GetAutocompleteAuthorNamesAsync([FromQuery, Required] string authorName, [BindTenant] TenantId tenant, CancellationToken ct)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead(tenant);

        var moduleIds = await unitOfRead.NexusModsArticles.GetAllModuleIdsAsync(authorName, ct);
        // TODO:

        return ApiResult(moduleIds);

        //return ApiResult(_dbContextRead.AutocompleteStartsWith<NexusModsArticleEntity>(x => x.NexusModsUser.Name.Name, authorName));
    }
}