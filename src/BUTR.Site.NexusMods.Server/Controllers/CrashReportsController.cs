using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Repositories;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.BindingSources;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;
using BUTR.Site.NexusMods.Server.Utils.Http.StreamingMultipartResults;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[TenantNotRequired]
[ApiController, Route("api/v1/[controller]"), ButrNexusModsAuthorization, TenantRequired]
public sealed class CrashReportsController : ApiControllerBase
{
    public sealed record CrashReportModel2
    {
        public required CrashReportId Id { get; init; }
        public required CrashReportVersion Version { get; init; }
        public required GameVersion GameVersion { get; init; }
        public required ExceptionTypeId ExceptionType { get; init; }
        public required string Exception { get; init; }
        public required DateTimeOffset Date { get; init; }
        public required CrashReportUrl Url { get; init; }
        public required ModuleId[] InvolvedModules { get; init; }
        public CrashReportStatus Status { get; init; } = CrashReportStatus.New;
        public string Comment { get; init; } = string.Empty;
    }


    private readonly ILogger _logger;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public CrashReportsController(ILogger<CrashReportsController> logger, IUnitOfWorkFactory unitOfWorkFactory)
    {
        _logger = logger;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    [HttpPatch]
    public async Task<ApiResult<string?>> UpdateAsync([FromQuery, Required] CrashReportId crashReportId, [FromQuery] CrashReportStatus? status, [FromQuery] string? comment, [BindUserId] NexusModsUserId userId, [BindTenant] TenantId tenant)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();
        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite();

        var existingEntity = await unitOfWrite.NexusModsUserToCrashReports.FirstOrDefaultAsync(
            x => x.TenantId == tenant && x.NexusModsUser.NexusModsUserId == userId && x.CrashReportId == crashReportId,
            null, CancellationToken.None);
        var entity = new NexusModsUserToCrashReportEntity
        {
            TenantId = tenant,
            NexusModsUserId = userId,
            NexusModsUser = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsUser(userId),
            CrashReportId = crashReportId,
            Status = status ?? existingEntity?.Status ?? CrashReportStatus.New,
            Comment = comment ?? existingEntity?.Comment ?? string.Empty,
        };
        unitOfWrite.NexusModsUserToCrashReports.Upsert(entity);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return ApiResult("Updated successful!");
    }

    private async Task<Paging<UserCrashReportModel>> GetPaginatedBaseAsync(NexusModsUserId userId, PaginatedQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var pageSize = Math.Max(Math.Min(query.PageSize, 50), 5);
        var filters = query.Filters ?? [];
        var sortings = query.Sortings is null || query.Sortings.Count == 0
            ? new List<Sorting> { new() { Property = nameof(CrashReportEntity.CreatedAt), Type = SortingType.Descending } }
            : query.Sortings;

        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var user = await unitOfRead.NexusModsUsers.GetUserAsync(userId, ct);
        return await unitOfRead.CrashReports.GetCrashReportsPaginatedAsync(user!, new PaginatedQuery(page, pageSize, filters, sortings), HttpContext.GetRole(), ct);
    }

    [HttpPost("Paginated")]
    public async Task<ApiResult<PagingData<CrashReportModel2>?>> GetPaginatedAsync([FromBody, Required] PaginatedQuery query, [BindUserId] NexusModsUserId userId, CancellationToken ct)
    {
        var paginated = await GetPaginatedBaseAsync(userId, query, ct);
        return ApiPagingResult(paginated, items => items.Select(x => new CrashReportModel2
        {
            Id = x.Id,
            Version = x.Version,
            GameVersion = x.GameVersion,
            ExceptionType = x.ExceptionType,
            Exception = x.Exception,
            Date = x.CreatedAt,
            Url = x.Url,
            InvolvedModules = x.InvolvedModuleIds,
            Status = x.Status,
            Comment = x.Comment ?? string.Empty,
        }));
    }

    [HttpPost("PaginatedStreaming")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<StreamingMultipartResult> GetPaginatedStreamingAsync([FromBody, Required] PaginatedQuery query, [BindUserId] NexusModsUserId userId, CancellationToken ct)
    {
        var paginated = await GetPaginatedBaseAsync(userId, query, ct);
        return ApiPagingStreamingResult(paginated, items => items.Select(x => new CrashReportModel2
        {
            Id = x.Id,
            Version = x.Version,
            GameVersion = x.GameVersion,
            ExceptionType = x.ExceptionType,
            Exception = x.Exception,
            Date = x.CreatedAt,
            Url = x.Url,
            InvolvedModules = x.InvolvedModuleIds,
            Status = x.Status,
            Comment = x.Comment ?? string.Empty,
        }));
    }

    [HttpGet("Autocomplete/ModuleIds")]
    public async Task<ApiResult<IList<string>?>> GetAutocompleteModuleIdsAsync([FromQuery, Required] ModuleId moduleId)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead();

        var moduleIds = await unitOfRead.Autocompletes.AutocompleteStartsWithAsync<CrashReportToModuleMetadataEntity, ModuleId>(x => x.Module.ModuleId, moduleId, CancellationToken.None);

        return ApiResult(moduleIds);
    }
}