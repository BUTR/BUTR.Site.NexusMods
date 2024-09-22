using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record CrashReportToMetadataEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }

    public required CrashReportId CrashReportId { get; init; }
    public CrashReportEntity? ToCrashReport { get; init; }

    public required string? LauncherType { get; init; }
    public required string? LauncherVersion { get; init; }

    public required string? Runtime { get; init; }

    public required string? BUTRLoaderVersion { get; init; }

    public required string? BLSEVersion { get; init; }
    public required string? LauncherExVersion { get; init; }
    
    public required string? OperatingSystemType { get; init; }
    public required string? OperatingSystemVersion { get; init; }


    public override int GetHashCode() => HashCode.Combine(TenantId, CrashReportId, LauncherType, LauncherVersion, Runtime, BUTRLoaderVersion, BLSEVersion, LauncherExVersion);
}