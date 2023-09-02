using BUTR.Site.NexusMods.Shared;

using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsArticleEntity : IEntityWithTenant
{
    public required Tenant TenantId { get; init; }

    public required ushort NexusModsArticleId { get; init; }

    public required string Title { get; init; }

    public required NexusModsUserEntity NexusModsUser { get; init; }

    public required DateTime CreateDate { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, NexusModsArticleId, Title, NexusModsUser.NexusModsUserId, CreateDate);
}