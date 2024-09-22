using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BUTR.Site.NexusMods.Server.Models.Database;

/// <summary>
/// Building block
/// </summary>
public sealed record ExceptionTypeEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }

    public required ExceptionTypeId ExceptionTypeId { get; init; }
    public ICollection<CrashReportEntity> ToCrashReports { get; init; } = new List<CrashReportEntity>();
    public ICollection<StatisticsTopExceptionsTypeEntity> ToTopExceptionsTypes { get; init; } = new List<StatisticsTopExceptionsTypeEntity>();

    public override int GetHashCode() => HashCode.Combine(TenantId, ExceptionTypeId);


    private ExceptionTypeEntity() { }
    [SetsRequiredMembers]
    private ExceptionTypeEntity(TenantId tenant, ExceptionTypeId exceptionTypeId) : this() => (TenantId, ExceptionTypeId) = (tenant, exceptionTypeId);

    public static ExceptionTypeEntity Create(TenantId tenant, ExceptionTypeId exceptionTypeId) => new(tenant, exceptionTypeId);
}