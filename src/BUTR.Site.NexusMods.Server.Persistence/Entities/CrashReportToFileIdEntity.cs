using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record CrashReportToFileIdEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }

    public required CrashReportId CrashReportId { get; init; }
    public CrashReportEntity? ToCrashReport { get; init; }

    public required CrashReportFileId FileId { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, CrashReportId, FileId);
}