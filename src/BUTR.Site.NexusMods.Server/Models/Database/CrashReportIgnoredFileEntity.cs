using BUTR.Site.NexusMods.Shared;

using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record CrashReportIgnoredFileEntity : IEntityWithTenant
{
    public required Tenant TenantId { get; init; }

    public required string Value { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, Value);
}