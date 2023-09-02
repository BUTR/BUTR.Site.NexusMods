using BUTR.Site.NexusMods.Shared;

using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record AutocompleteEntity : IEntityWithTenant
{
    public required Tenant TenantId { get; init; }

    public required int AutocompleteId { get; init; }
    public required string Type { get; init; }
    public required string Value { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, AutocompleteId, Type, Value);
}