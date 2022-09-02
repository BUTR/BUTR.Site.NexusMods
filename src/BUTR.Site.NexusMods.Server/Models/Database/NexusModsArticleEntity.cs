using System;

namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record NexusModsArticleEntity : IEntity
    {
        public int ArticleId { get; set; } = default!;

        public string Title { get; set; } = default!;

        public int AuthorId { get; set; } = default!;
        public string AuthorName { get; set; } = default!;

        public DateTime CreateDate { get; set; } = default!;
    }
}