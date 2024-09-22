using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

public sealed record ModuleIdToVersionModel
{
    public required ModuleId ModuleId { get; init; }
    public required ModuleVersion Version { get; init; }
}
public sealed record UserCrashReportModel
{
    public required CrashReportId Id { get; init; }
    public required CrashReportVersion Version { get; init; }
    public required GameVersion GameVersion { get; init; }
    public required ExceptionTypeId ExceptionType { get; init; }
    public required string Exception { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    //public required ModuleId[] ModuleIds { get; init; }
    //public required ModuleIdToVersionModel[] ModuleIdToVersion { get; init; }
    public required ModuleId? TopInvolvedModuleId { get; init; } // Used for FE search
    public required ModuleId[] InvolvedModuleIds { get; init; }
    //public required NexusModsModId[] NexusModsModIds { get; init; }
    public required CrashReportUrl Url { get; init; }

    public required CrashReportStatus Status { get; init; }
    public required string? Comment { get; init; }
}

public interface ICrashReportEntityRepositoryRead : IRepositoryRead<CrashReportEntity>
{
    Task<Paging<UserCrashReportModel>> GetCrashReportsPaginatedAsync(NexusModsUserEntity user, PaginatedQuery query, ApplicationRole applicationRole, CancellationToken ct);
}

public interface ICrashReportEntityRepositoryWrite : IRepositoryWrite<CrashReportEntity>, ICrashReportEntityRepositoryRead
{
    Task GenerateAutoCompleteForGameVersionsAsync(CancellationToken ct);
}