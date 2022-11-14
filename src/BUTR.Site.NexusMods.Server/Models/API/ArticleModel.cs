using System;

namespace BUTR.Site.NexusMods.Server.Models.API
{
    public record ArticleModel(int Id, string Title, int AuthorId, string AuthorName, DateTimeOffset CreateDate);
}