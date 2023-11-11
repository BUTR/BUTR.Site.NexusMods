using BUTR.Site.NexusMods.Server.Extensions;

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
    public static ExceptionTypeEntity FromException(TenantId tenant, string exception)
    {
        var exceptionType = "NO_EXCEPTION";
        Span<Range> dest = stackalloc Range[32];
        var idx = 0;
        foreach (ReadOnlySpan<char> line in exception.SplitLines())
        {
            if (idx < 3)
            {
                idx++;
                continue;
            }

            var count = line.Split(dest, ':');
            if (count < 2) break;
            exceptionType = line[dest[1]].Trim().ToString();
            break;
        }
        return new() { TenantId = tenant, ExceptionTypeId = ExceptionTypeId.From(exceptionType) };
    }
}