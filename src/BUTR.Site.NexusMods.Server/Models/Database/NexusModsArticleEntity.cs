using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsArticleEntity : IEntity
{
    public required int NexusModsArticleId { get; init; }

    public required string Title { get; init; }

    public required int NexusModsUserId { get; init; }
    public required string AuthorName { get; init; }

    public required DateTime CreateDate { get; init; }
}