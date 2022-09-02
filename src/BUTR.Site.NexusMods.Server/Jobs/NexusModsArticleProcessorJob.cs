using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Services;

using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Jobs
{
    [DisallowConcurrentExecution]
    public sealed class NexusModsArticleProcessorJob : IJob
    {
        private readonly ILogger _logger;
        private readonly NexusModsClient _client;
        private readonly AppDbContext _dbContext;

        public NexusModsArticleProcessorJob(ILogger<NexusModsArticleProcessorJob> logger, NexusModsClient client, AppDbContext dbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            const string gameDomain = "mountandblade2bannerlord";
            const int notFoundArticlesTreshold = 20;

            var articleId = 0;
            var notFoundArticles = 0;
            while (true)
            {
                var articleDocument = await _client.GetArticleAsync(gameDomain, articleId);
                if (articleDocument is null) continue;


                var errorElement = articleDocument.GetElementbyId($"{articleId}-title");
                if (errorElement is not null)
                {
                    notFoundArticles++;
                    articleId++;
                    if (notFoundArticles >= notFoundArticlesTreshold)
                    {
                        break;
                    }
                    continue;
                }
                notFoundArticles = 0;

                var pagetitleElement = articleDocument.GetElementbyId("pagetitle");
                var titleElement = pagetitleElement.ChildNodes.FindFirst("h1");
                var title = titleElement.InnerText;

                var authorElement = articleDocument.GetElementbyId("image-author-name");
                var authorUrl = authorElement.GetAttributeValue("href", "0");
                var authorUrlSplit = authorUrl.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var authorIdText = authorUrlSplit.LastOrDefault() ?? string.Empty;
                var authorId = int.TryParse(authorIdText, out var authorVal) ? authorVal : 0;
                var authorText = authorElement.InnerText;

                var fileinfoElement = articleDocument.GetElementbyId("fileinfo");
                var dateTimeText1 = fileinfoElement.ChildNodes.FindFirst("div");
                var dateTimeText2 = dateTimeText1?.ChildNodes.FindFirst("time");
                var dateTimeText = dateTimeText2?.GetAttributeValue("datetime", "");
                var dateTime = DateTimeOffset.TryParse(dateTimeText, out var dateTimeVal) ? dateTimeVal.UtcDateTime : DateTimeOffset.MinValue.UtcDateTime;

                NexusModsArticleEntity? ApplyChanges(NexusModsArticleEntity? existing) => existing switch
                {
                    null => new()
                    {
                        Title = title,
                        ArticleId = articleId,
                        AuthorId = authorId,
                        AuthorName = authorText,
                        CreateDate = dateTime
                    },
                    var entity => entity with
                    {
                        Title = title
                    }
                };
                await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsArticleEntity>(x => x.ArticleId == articleId, ApplyChanges, context.CancellationToken);
            }
        }
    }
}