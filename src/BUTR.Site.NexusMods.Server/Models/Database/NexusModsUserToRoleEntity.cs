using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsUserToRoleEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }

    public required NexusModsUserEntity NexusModsUser { get; init; }

    public required ApplicationRole Role { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, NexusModsUser.NexusModsUserId, Role);
}