using BUTR.Site.NexusMods.Shared;

using System;
using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record CrashReportEntity : IEntityWithTenant
{
    public required Tenant TenantId { get; init; }

    public required Guid CrashReportId { get; init; }
    public CrashReportToMetadataEntity? Metadata { get; init; }
    public CrashReportToFileIdEntity? FileId { get; init; }
    public ICollection<CrashReportToModuleMetadataEntity> ModuleInfos { get; init; } = new List<CrashReportToModuleMetadataEntity>();
    public ICollection<NexusModsUserToCrashReportEntity> ToUsers { get; init; } = new List<NexusModsUserToCrashReportEntity>();

    public required byte Version { get; init; }

    public required string GameVersion { get; init; } = string.Empty;

    public required ExceptionTypeEntity ExceptionType { get; init; }

    public required string Exception { get; init; } = string.Empty;

    public required DateTime CreatedAt { get; init; } = DateTime.MinValue;

    public required string Url { get; init; } = string.Empty;

    public override int GetHashCode() => HashCode.Combine(CrashReportId, Version, GameVersion, ExceptionType, Exception, CreatedAt, Url);
}