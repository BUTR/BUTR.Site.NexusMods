using BUTR.Site.NexusMods.Shared;

using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record CrashReportToMetadataEntity : IEntityWithTenant
{
    public required Tenant TenantId { get; init; }

    public required Guid CrashReportId { get; init; }
    public CrashReportEntity? ToCrashReport { get; init; }

    public required string? LauncherType { get; init; }
    public required string? LauncherVersion { get; init; }

    public required string? Runtime { get; init; }

    public required string? BUTRLoaderVersion { get; init; }

    public required string? BLSEVersion { get; init; }
    public required string? LauncherExVersion { get; init; }

    public override int GetHashCode() => HashCode.Combine(CrashReportId, LauncherType, LauncherVersion, Runtime, BUTRLoaderVersion, BLSEVersion, LauncherExVersion);
}