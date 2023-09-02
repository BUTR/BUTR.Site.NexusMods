using BUTR.Site.NexusMods.Shared;

using System;
using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record StatisticsTopExceptionsTypeEntity : IEntityWithTenant
{
    public required Tenant TenantId { get; init; }

    public required ExceptionTypeEntity ExceptionType { get; init; }

    public required int ExceptionCount { get; init; }

    public override int GetHashCode() => HashCode.Combine(ExceptionType, ExceptionCount);
}