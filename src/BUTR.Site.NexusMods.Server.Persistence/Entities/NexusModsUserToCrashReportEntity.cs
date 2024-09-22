using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsUserToCrashReportEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }

    public required NexusModsUserId NexusModsUserId { get; init; }
    public required NexusModsUserEntity NexusModsUser { get; init; }

    public required CrashReportId CrashReportId { get; init; }
    public CrashReportEntity? ToCrashReport { get; init; }

    public required CrashReportStatus Status { get; init; }

    public required string Comment { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, NexusModsUserId, CrashReportId, Status, Comment);
}