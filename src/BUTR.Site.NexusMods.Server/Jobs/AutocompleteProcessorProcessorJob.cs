using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Jobs;

[DisallowConcurrentExecution]
public sealed class AutocompleteProcessorProcessorJob : IJob
{
    private sealed record AutocompleteEntry(string Name, Func<IAppDbContextRead, IQueryable<string>> Query);

    /*
    private sealed record GroupingEntry
    {
        public required string Key { get; init; }
        public required string[] Values { get; init; }
    }
    private sealed record AutocompleteGroupingEntry(string Name, Func<IAppDbContextRead, IQueryable<GroupingEntry>> Query);
    */

    private static readonly AutocompleteEntry[] ToAutocomplete =
    {
        new(GenerateName<CrashReportEntity, GameVersion>(x => x.GameVersion), x => x.CrashReports.Select(y => y.GameVersion.Value)),
        new(GenerateName<CrashReportToModuleMetadataEntity, ModuleId>(x => x.Module.ModuleId), x => x.CrashReportModuleInfos.Select(y => y.Module.ModuleId.Value)),
        new(GenerateName<NexusModsArticleEntity, NexusModsUserName>(x => x.NexusModsUser.Name!.Name), x => x.NexusModsArticles.Include(y => y.NexusModsUser).ThenInclude(y => y.Name).Select(y => y.NexusModsUser).Select(y => y.Name!).Select(y => y.Name.Value)),
    };

    //private static readonly AutocompleteGroupingEntry[] ToAutocompleteGrouping =
    //{
    //    new(GenerateName<CrashReportToModuleMetadataEntity, ModuleId>(x => x.Module.ModuleId),
    //        x => x.CrashReportModuleInfos.GroupBy(y => y.Module.ModuleId).Select(y => new GroupingEntry { Key = y.Key, Values = y.Select(z => z.Version).Distinct().ToArray() })),
    //};

    public static string GenerateName<TEntity, TParameter>(Expression<Func<TEntity, TParameter>> property)
    {
        if (property is not LambdaExpression { Body: MemberExpression { Member: PropertyInfo propertyInfo } }) return string.Empty;
        return $"{propertyInfo.DeclaringType!.Name}.{propertyInfo.Name}";
    }

    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AutocompleteProcessorProcessorJob(ILogger<AutocompleteProcessorProcessorJob> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var ct = context.CancellationToken;

        foreach (var tenant in TenantId.Values)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();

            var tenantContextAccessor = scope.ServiceProvider.GetRequiredService<ITenantContextAccessor>();
            tenantContextAccessor.Current = tenant;

            await HandleTenantAsync(tenant, scope.ServiceProvider, ct);
        }

        context.Result = "Updated Autocomplete Data";
        context.SetIsSuccess(true);
    }

    private static async Task HandleTenantAsync(TenantId tenant, IServiceProvider serviceProvider, CancellationToken ct)
    {
        var dbContextRead = serviceProvider.GetRequiredService<IAppDbContextRead>();
        var dbContextWrite = serviceProvider.GetRequiredService<IAppDbContextWrite>();

        foreach (var autocompleteEntry in ToAutocomplete)
        {
            if (ct.IsCancellationRequested) return;

            var key = autocompleteEntry.Name;

            await dbContextWrite.Autocompletes.Where(x => x.Type == key).ExecuteDeleteAsync(ct);
            await dbContextWrite.Autocompletes.UpsertAsync(autocompleteEntry.Query(dbContextRead).Distinct().Select(x => new AutocompleteEntity
            {
                TenantId = tenant,
                Type = key,
                Value = x,
            }));
        }

        /*
        foreach (var autocompleteGroupingEntry in ToAutocompleteGrouping)
        {
            foreach (var chunk in autocompleteGroupingEntry.Query(dbContextRead).AsEnumerable().Chunk(50))
            {
                var keys = chunk.Select(x => $"{autocompleteGroupingEntry.Name}.{x.Key}").ToArray();
                foreach (var groupingEntry in chunk)
                {
                    if (ct.IsCancellationRequested) return;

                    var key = $"{autocompleteGroupingEntry.Name}.{groupingEntry.Key}";
                    await dbContextWrite.Autocompletes.AddRangeAsync(groupingEntry.Values.Select(x => new AutocompleteEntity
                    {
                        TenantId = tenant,
                        AutocompleteId = 0,
                        Type = key,
                        Value = x,
                    }), ct);
                }

                await dbContextWrite.Autocompletes.Where(x => keys.Contains(x.Type)).ExecuteDeleteAsync(ct);
                await dbContextWrite.SaveAsync(ct);
            }
        }
        */
    }
}