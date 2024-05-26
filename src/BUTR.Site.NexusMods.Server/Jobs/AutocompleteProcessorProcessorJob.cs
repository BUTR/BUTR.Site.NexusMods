using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Repositories;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Jobs;

[DisallowConcurrentExecution]
public sealed class AutocompleteProcessorProcessorJob : IJob
{
    /*
    private sealed record AutocompleteEntry(string Name, Func<IUnitOfRead, IQueryable<string>> Query);

    private sealed record GroupingEntry
    {
        public required string Key { get; init; }
        public required string[] Values { get; init; }
    }
    private sealed record AutocompleteGroupingEntry(string Name, Func<IUnitOfRead, IQueryable<GroupingEntry>> Query);

    private static readonly AutocompleteEntry[] ToAutocomplete =
    [
        new(GenerateName<CrashReportEntity, GameVersion>(x => x.GameVersion), x => x.CrashReports.GetAllGameVersions().Select(y => y.Value)),
        new(GenerateName<CrashReportToModuleMetadataEntity, ModuleId>(x => x.Module.ModuleId), x => x.CrashReportModuleInfos.GetAllModuleIds().Select(y => y.Value)),
        new(GenerateName<NexusModsArticleEntity, NexusModsUserName>(x => x.NexusModsUser.Name!.Name), x => x.NexusModsArticles.GetAllUserNames().Select(y => y.Value))
    ];

    private static readonly AutocompleteGroupingEntry[] ToAutocompleteGrouping =
    [
        new(GenerateName<CrashReportToModuleMetadataEntity, ModuleId>(x => x.Module.ModuleId),
            x => x.CrashReportModuleInfos.GroupBy(y => y.Module.ModuleId).Select(y => new GroupingEntry { Key = y.Key, Values = y.Select(z => z.Version).Distinct().ToArray() })),
    ];
    */

    public static string GenerateName<TEntity, TParameter>(Expression<Func<TEntity, TParameter>> property)
    {
        if (property is not LambdaExpression { Body: MemberExpression { Member: PropertyInfo propertyInfo } }) return string.Empty;
        return $"{propertyInfo.DeclaringType!.Name}.{propertyInfo.Name}";
    }

    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AutocompleteProcessorProcessorJob(ILogger<AutocompleteProcessorProcessorJob> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var ctsTimeout = new CancellationTokenSource(TimeSpan.FromMinutes(10));
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, ctsTimeout.Token);
        var ct = cts.Token;

        foreach (var tenant in TenantId.Values)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope().WithTenant(tenant);
            await HandleTenantAsync(scope, ct);
        }

        context.Result = "Updated Autocomplete Data";
        context.SetIsSuccess(true);
    }

    private async Task HandleTenantAsync(AsyncServiceScope scope, CancellationToken ct)
    {
        var unitOfWorkFactory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();
        await using var unitOfWrite = unitOfWorkFactory.CreateUnitOfWrite();

        await unitOfWrite.NexusModsArticles.GenerateAutoCompleteForAuthorNameAsync(ct);
        await unitOfWrite.CrashReports.GenerateAutoCompleteForGameVersionsAsync(ct);
        await unitOfWrite.CrashReportModuleInfos.GenerateAutoCompleteForModuleIdsAsync(ct);

        await unitOfWrite.SaveChangesAsync(ct);
    }
}