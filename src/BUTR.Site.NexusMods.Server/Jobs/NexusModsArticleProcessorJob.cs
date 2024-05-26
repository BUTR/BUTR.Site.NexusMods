using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Repositories;
using BUTR.Site.NexusMods.Server.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Jobs;

[DisallowConcurrentExecution]
public sealed class NexusModsArticleProcessorJob : IJob
{
    private readonly ILogger _logger;
    private readonly INexusModsClient _nexusModsClient;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public NexusModsArticleProcessorJob(ILogger<NexusModsArticleProcessorJob> logger, INexusModsClient nexusModsClient, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _nexusModsClient = nexusModsClient;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var ctsTimeout = new CancellationTokenSource(TimeSpan.FromMinutes(30));
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, ctsTimeout.Token);
        var ct = cts.Token;

        foreach (var tenant in TenantId.Values)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope().WithTenant(tenant);
            await HandleTenantAsync(scope, tenant, ct);
        }

        context.Result = "Processed all available articles";
        context.SetIsSuccess(true);
    }

    private async Task HandleTenantAsync(AsyncServiceScope scope, TenantId tenant, CancellationToken ct)
    {
        const int notFoundArticlesTreshold = 50;

        var unitOfWorkFactory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();
        await using var unitOfWrite = unitOfWorkFactory.CreateUnitOfWrite();

        var gameDomain = tenant.ToGameDomain();

        var articleIdRaw = 0;
        var notFoundArticles = 0;
        var @break = false;
        while (!ct.IsCancellationRequested && !@break)
        {
            var articles = ImmutableArray.CreateBuilder<NexusModsArticleEntity>();

            for (var i = 0; i < 50; i++)
            {
                var articleId = NexusModsArticleId.From(articleIdRaw);

                if (await _nexusModsClient.GetArticleAsync(gameDomain, articleId, ct) is not { } articleDocument)
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
                        @break = true;
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
                var dateTime = DateTimeOffset.TryParse(dateTimeText, out var dateTimeVal) ? dateTimeVal.ToUniversalTime() : DateTimeOffset.MinValue.ToUniversalTime();

                articles.Add(new()
                {
                    TenantId = tenant,
                    Title = title,
                    NexusModsArticleId = articleId,
                    NexusModsUserId = authorId,
                    NexusModsUser = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsUserWithName(authorId, NexusModsUserName.From(authorText)),
                    CreateDate = dateTime
                });
                articleIdRaw++;
            }

            unitOfWrite.NexusModsArticles.UpsertRange(articles);
            await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        }
    }
}