using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record AutocompleteEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }

    public required string Type { get; init; }
    public required string Value { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, Type, Value);
}