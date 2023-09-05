using System;
using System.Diagnostics.CodeAnalysis;

namespace BUTR.Site.NexusMods.Server.Models.Database;

/// <summary>
/// Building block
/// </summary>
public record TenantEntity : IEntity
{
    public required TenantId TenantId { get; init; }
    public override int GetHashCode() => HashCode.Combine(TenantId);


    private TenantEntity() { }
    [SetsRequiredMembers]
    private TenantEntity(TenantId tenant) : this() => TenantId = tenant;

    public static TenantEntity Create(TenantId tenant) => new(tenant);
}