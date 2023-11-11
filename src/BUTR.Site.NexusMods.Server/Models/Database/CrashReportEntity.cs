using System;
using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record CrashReportEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }

    public required CrashReportId CrashReportId { get; init; }
    public CrashReportToMetadataEntity? Metadata { get; init; }
    public CrashReportToFileIdEntity? FileId { get; init; }
    public ICollection<CrashReportToModuleMetadataEntity> ModuleInfos { get; init; } = new List<CrashReportToModuleMetadataEntity>();
    public ICollection<NexusModsUserToCrashReportEntity> ToUsers { get; init; } = new List<NexusModsUserToCrashReportEntity>();

    public required CrashReportVersion Version { get; init; }

    public required GameVersion GameVersion { get; init; }

    public required ExceptionTypeEntity ExceptionType { get; init; }

    public required string Exception { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required CrashReportUrl Url { get; init; }

    public override int GetHashCode() => HashCode.Combine(CrashReportId, Version, GameVersion, ExceptionType, Exception, CreatedAt, Url);
}