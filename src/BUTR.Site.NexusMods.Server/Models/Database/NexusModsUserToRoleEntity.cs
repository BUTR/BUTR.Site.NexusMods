﻿using BUTR.Site.NexusMods.Shared;

using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsUserToRoleEntity : IEntityWithTenant
{
    public required Tenant TenantId { get; init; }

    public required NexusModsUserEntity NexusModsUser { get; init; }

    public required string Role { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, NexusModsUser.NexusModsUserId, Role);
}