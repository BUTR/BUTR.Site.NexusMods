using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Shared;

using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsUserToCrashReportEntity : IEntityWithTenant
{
    public required Tenant TenantId { get; init; }

    public required NexusModsUserEntity NexusModsUser { get; init; }

    public required Guid CrashReportId { get; init; }
    public CrashReportEntity? ToCrashReport { get; init; }

    public required CrashReportStatus Status { get; init; }

    public required string Comment { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, NexusModsUser.NexusModsUserId, CrashReportId, Status, Comment);
}