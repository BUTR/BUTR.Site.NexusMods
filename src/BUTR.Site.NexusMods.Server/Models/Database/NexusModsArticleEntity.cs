using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsArticleEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }

    public required NexusModsArticleId NexusModsArticleId { get; init; }

    public required string Title { get; init; }

    public required NexusModsUserEntity NexusModsUser { get; init; }

    public required DateTimeOffset CreateDate { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, NexusModsArticleId, Title, NexusModsUser.NexusModsUserId, CreateDate);
}