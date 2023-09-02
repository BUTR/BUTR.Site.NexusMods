using BUTR.Site.NexusMods.Shared;

using System;
using System.Diagnostics.CodeAnalysis;

namespace BUTR.Site.NexusMods.Server.Models.Database;

/// <summary>
/// Building block
/// </summary>
public record TenantEntity : IEntity
{
    public required Tenant TenantId { get; init; }
    public override int GetHashCode() => HashCode.Combine(TenantId);


    private TenantEntity() { }
    [SetsRequiredMembers]
    private TenantEntity(Tenant tenant) : this() => TenantId = tenant;

    public static TenantEntity Create(Tenant tenant) => new(tenant);
}