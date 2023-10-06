using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Jobs;

[DisallowConcurrentExecution]
public sealed class NexusModsArticleUpdatesProcessorJob : IJob
{
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public NexusModsArticleUpdatesProcessorJob(ILogger<NexusModsArticleUpdatesProcessorJob> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var ct = context.CancellationToken;

        var processed = 0;
        foreach (var tenant in TenantId.Values)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();

            var tenantContextAccessor = scope.ServiceProvider.GetRequiredService<ITenantContextAccessor>();
            tenantContextAccessor.Current = tenant;

            processed += await HandleTenantAsync(tenant, scope.ServiceProvider, ct);
        }

        context.Result = $"Processed {processed} article updates";
        context.SetIsSuccess(true);
    }

    private static async Task<int> HandleTenantAsync(TenantId tenant, IServiceProvider serviceProvider, CancellationToken ct)
    {
        const int notFoundArticlesTreshold = 50;

        var client = serviceProvider.GetRequiredService<NexusModsClient>();
        var dbContextRead = serviceProvider.GetRequiredService<IAppDbContextRead>();
        var dbContextWrite = serviceProvider.GetRequiredService<IAppDbContextWrite>();
        var entityFactory = dbContextWrite.CreateEntityFactory();
        await using var _ = dbContextWrite.CreateSaveScope();

        var gameDomain = tenant.ToGameDomain();

        var articles = new List<NexusModsArticleEntity>();

        var articleIdRaw = dbContextRead.NexusModsArticles.OrderBy(x => x.NexusModsArticleId).LastOrDefault()?.NexusModsArticleId.Value ?? 0;
        var notFoundArticles = 0;
        var processed = 0;
        while (!ct.IsCancellationRequested)
        {
            var articleId = NexusModsArticleId.From(articleIdRaw);

            if (await client.GetArticleAsync(gameDomain, articleId, ct) is not { } articleDocument)
            {
                articleIdRaw++;
                continue;
            }

            if (articleDocument.GetElementbyId($"{articleId}-title") is not null)
            {
                notFoundArticles++;
                articleIdRaw++;
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
            var authorId = NexusModsUserId.TryParse(authorIdText, out var val) ? val : throw new Exception("Author Id invalid");
            var authorText = authorElement.InnerText;

            var fileinfoElement = articleDocument.GetElementbyId("fileinfo");
            var dateTimeText1 = fileinfoElement.ChildNodes.FindFirst("div");
            var dateTimeText2 = dateTimeText1?.ChildNodes.FindFirst("time");
            var dateTimeText = dateTimeText2?.GetAttributeValue("datetime", "");
            var dateTime = DateTimeOffset.TryParse(dateTimeText, out var dateTimeVal) ? dateTimeVal.UtcDateTime : DateTimeOffset.MinValue.UtcDateTime;

            articles.Add(new()
            {
                TenantId = tenant,
                Title = title,
                NexusModsArticleId = articleId,
                NexusModsUser = entityFactory.GetOrCreateNexusModsUserWithName(authorId, NexusModsUserName.From(authorText)),
                CreateDate = dateTime
            });
            articleIdRaw++;
            processed++;
        }

        dbContextWrite.FutureUpsert(x => x.NexusModsArticles, articles);
        // Disposing the DBContext will save the data

        return processed;
    }
}