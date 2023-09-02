using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Shared;
using BUTR.Site.NexusMods.Shared.Extensions;

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
public sealed class NexusModsArticleProcessorJob : IJob
{
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public NexusModsArticleProcessorJob(ILogger<NexusModsArticleProcessorJob> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var ct = context.CancellationToken;
        var tenants = Enum.GetValues<Tenant>();

        foreach (var tenant in tenants)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();

            var tenantContextAccessor = scope.ServiceProvider.GetRequiredService<ITenantContextAccessor>();
            tenantContextAccessor.Current = tenant;

            await HandleTenantAsync(tenant, scope.ServiceProvider, ct);
        }

        context.Result = "Processed all available articles";
        context.SetIsSuccess(true);
    }

    private static async Task HandleTenantAsync(Tenant tenant, IServiceProvider serviceProvider, CancellationToken ct)
    {
        const int notFoundArticlesTreshold = 20;

        var client = serviceProvider.GetRequiredService<NexusModsClient>();
        var dbContextWrite = serviceProvider.GetRequiredService<IAppDbContextWrite>();
        var entityFactory = dbContextWrite.CreateEntityFactory();
        await using var _ = dbContextWrite.CreateSaveScope();

        var gameDomain = tenant.GameDomain();

        var articles = new List<NexusModsArticleEntity>();

        var articleId = 0;
        var notFoundArticles = 0;
        while (!ct.IsCancellationRequested)
        {
            var articleDocument = await client.GetArticleAsync(gameDomain, articleId, ct);
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

            articles.Add(new()
            {
                TenantId = tenant,
                Title = title,
                NexusModsArticleId = (ushort) articleId,
                NexusModsUser = entityFactory.GetOrCreateNexusModsUserWithName(authorId, authorText),
                CreateDate = dateTime
            });
            articleId++;
        }

        dbContextWrite.FutureUpsert(x => x.NexusModsArticles, articles);
        // Disposing the DBContext will save the data
    }
}