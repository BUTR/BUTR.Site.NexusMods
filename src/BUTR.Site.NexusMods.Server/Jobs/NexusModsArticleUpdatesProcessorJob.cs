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
    public sealed class NexusModsArticleUpdatesProcessorJob : IJob
    {
        private readonly ILogger _logger;
        private readonly NexusModsClient _client;
        private readonly AppDbContext _dbContext;

        public NexusModsArticleUpdatesProcessorJob(ILogger<NexusModsArticleUpdatesProcessorJob> logger, NexusModsClient client, AppDbContext dbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            const string gameDomain = "mountandblade2bannerlord";
            const int notFoundArticlesTreshold = 20;

            var ct = context.CancellationToken;

            var articleId = _dbContext.Set<NexusModsArticleEntity>().OrderBy(x => x.NexusModsArticleId).LastOrDefault()?.NexusModsArticleId ?? 0;
            var notFoundArticles = 0;
            var processed = 0;
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var articleDocument = await _client.GetArticleAsync(gameDomain, articleId, ct);
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
                            NexusModsArticleId = articleId,
                            NexusModsUserId = authorId,
                            AuthorName = authorText,
                            CreateDate = dateTime
                        },
                        _ => existing with
                        {
                            Title = title
                        }
                    };
                    await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsArticleEntity>(x => x.NexusModsArticleId == articleId, ApplyChanges, ct);
                    articleId++;
                    processed++;
                }
            }
            finally
            {
                context.Result = $"Processed {processed} article updates";
                context.SetIsSuccess(true);
            }
        }
    }
}