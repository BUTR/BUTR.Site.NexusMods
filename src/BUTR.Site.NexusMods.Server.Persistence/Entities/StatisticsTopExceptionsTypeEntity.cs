using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record StatisticsTopExceptionsTypeEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }

    public required ExceptionTypeId ExceptionTypeId { get; init; }
    public required ExceptionTypeEntity ExceptionType { get; init; }

    public required int ExceptionCount { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, ExceptionTypeId, ExceptionCount);
}