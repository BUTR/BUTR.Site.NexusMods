using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Shared;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BUTR.Site.NexusMods.Server.Models.Database;

/// <summary>
/// Building block
/// </summary>
public sealed record ExceptionTypeEntity : IEntityWithTenant
{
    public required Tenant TenantId { get; init; }

    public required string ExceptionTypeId { get; init; }
    public ICollection<CrashReportEntity> ToCrashReports { get; init; } = new List<CrashReportEntity>();
    public ICollection<StatisticsTopExceptionsTypeEntity> ToTopExceptionsTypes { get; init; } = new List<StatisticsTopExceptionsTypeEntity>();

    public override int GetHashCode() => HashCode.Combine(TenantId, ExceptionTypeId);


    private ExceptionTypeEntity() { }
    [SetsRequiredMembers]
    private ExceptionTypeEntity(Tenant tenant, string exceptionTypeId) : this() => (TenantId, ExceptionTypeId) = (tenant, exceptionTypeId);

    public static ExceptionTypeEntity Create(Tenant tenant, string exceptionTypeId) => new(tenant, exceptionTypeId);
    public static ExceptionTypeEntity FromException(Tenant tenant, string exception)
    {
        var exceptionType = "NO_EXCEPTION";
        Span<Range> dest = stackalloc Range[32];
        var idx = 0;
        foreach (ReadOnlySpan<char> line in exception.SplitLines())
        {
            if (idx < 3)
            {
                idx ++;
                continue;
            }

            var count = line.Split(dest, ':');
            if (count < 2) break;
            exceptionType = line[dest[1]].Trim().ToString();
            break;
        }
        return new() { TenantId = tenant, ExceptionTypeId = exceptionType };
    }
}