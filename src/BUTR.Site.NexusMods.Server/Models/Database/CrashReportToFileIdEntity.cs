using BUTR.Site.NexusMods.Shared;

using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record CrashReportToFileIdEntity : IEntityWithTenant
{
    public required Tenant TenantId { get; init; }

    public required Guid CrashReportId { get; init; }
    public CrashReportEntity? ToCrashReport { get; init; }

    public required string FileId { get; init; }

    public override int GetHashCode() => HashCode.Combine(CrashReportId, FileId);
}