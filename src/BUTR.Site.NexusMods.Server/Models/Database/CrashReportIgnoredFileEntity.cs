using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record CrashReportIgnoredFileEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }

    public required CrashReportFileId CrashReportFileId { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, CrashReportFileId);
}