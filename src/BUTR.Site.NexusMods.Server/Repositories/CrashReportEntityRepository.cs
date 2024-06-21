using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Jobs;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Shared.Helpers;

using EFCore.BulkExtensions;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface ICrashReportEntityRepositoryRead : IRepositoryRead<CrashReportEntity>
{
    Task<Paging<UserCrashReportModel>> GetCrashReportsPaginatedAsync(NexusModsUserEntity user, PaginatedQuery query, ApplicationRole applicationRole, CancellationToken ct);
}

public interface ICrashReportEntityRepositoryWrite : IRepositoryWrite<CrashReportEntity>, ICrashReportEntityRepositoryRead
{
    Task GenerateAutoCompleteForGameVersionsAsync(CancellationToken ct);
}

[ScopedService<ICrashReportEntityRepositoryWrite, ICrashReportEntityRepositoryRead>]
internal class CrashReportEntityRepository : Repository<CrashReportEntity>, ICrashReportEntityRepositoryWrite
{
    private readonly ITenantContextAccessor _tenantContextAccessor;

    protected override IQueryable<CrashReportEntity> InternalQuery => base.InternalQuery
        .Include(x => x.ExceptionType)
        .Include(x => x.FileId)
        .Include(x => x.Metadata)
        .Include(x => x.ModuleInfos)
        .Include(x => x.ToUsers);

    public CrashReportEntityRepository(IAppDbContextProvider appDbContextProvider, ITenantContextAccessor tenantContextAccessor) : base(appDbContextProvider.Get())
    {
        _tenantContextAccessor = tenantContextAccessor;
    }

    public async Task GenerateAutoCompleteForGameVersionsAsync(CancellationToken ct)
    {
        var tenant = _tenantContextAccessor.Current;
        var key = AutocompleteProcessorProcessorJob.GenerateName<CrashReportEntity, GameVersion>(x => x.GameVersion);

        await _dbContext.Autocompletes.Where(x => x.Type == key).ExecuteDeleteAsync(ct);

        var data = await _dbContext.CrashReports.Select(x => x.GameVersion.Value).Distinct().Select(x => new AutocompleteEntity
        {
            AutocompleteId = default,
            TenantId = tenant,
            Type = key,
            Value = x,
        }).ToListAsync(ct);
        await _dbContext.BulkInsertOrUpdateAsync(data, cancellationToken: ct);
    }

    public async Task<Paging<UserCrashReportModel>> GetCrashReportsPaginatedAsync(NexusModsUserEntity user, PaginatedQuery query, ApplicationRole applicationRole, CancellationToken ct)
    {
        var moduleIds = user.ToModules.Select(x => x.Module.ModuleId).ToHashSet();
        var nexusModsModIds = user.ToNexusModsMods.Select(x => x.NexusModsMod.NexusModsModId).ToHashSet();

        IQueryable<UserCrashReportModel> DbQueryBase(Expression<Func<CrashReportEntity, bool>> predicate) => _dbContext.CrashReports
            .Include(x => x.ToUsers).ThenInclude(x => x.NexusModsUser)
            .Include(x => x.ModuleInfos).ThenInclude(x => x.Module)
            .Include(x => x.ModuleInfos).ThenInclude(x => x.NexusModsMod)
            .Include(x => x.ModuleInfos).ThenInclude(x => x.Module)
            .Include(x => x.ExceptionType)
            .AsSplitQuery()
            //.Where(x => x.CreatedAt > DateTimeOffset.Parse("2020-03-31 00:00:00"))
            //.Where(x => (byte) x.Version > 10)
            .Where(predicate)
            .Select(x => new UserCrashReportModel
            {
                Id = x.CrashReportId,
                Version = x.Version,
                GameVersion = x.GameVersion,
                ExceptionType = x.ExceptionType.ExceptionTypeId,
                Exception = x.Exception,
                CreatedAt = x.CreatedAt,
                Url = x.Url,
                //ModuleIds = x.ModuleInfos.Select(y => y.Module).Select(y => y.ModuleId).ToArray(),
                //ModuleIdToVersion = x.ModuleInfos.Select(y => new ModuleIdToVersionView { ModuleId = y.Module.ModuleId, Version = y.Version }).ToArray(),
                //TopInvolvedModuleId = x.ModuleInfos.OrderBy(y => y.InvolvedPosition).Where(z => z.IsInvolved).Select(y => y.Module).Select(y => y.ModuleId).Cast<ModuleId?>().FirstOrDefault(),
                InvolvedModuleIds = x.ModuleInfos.OrderBy(y => y.InvolvedPosition).Where(z => z.IsInvolved).Select(y => y.Module).Select(y => y.ModuleId).ToArray(),
                //NexusModsModIds = x.ModuleInfos.Select(y => y.NexusModsMod).Where(y => y != null).Select(y => y!.NexusModsModId).ToArray(),
                Status = x.ToUsers.Where(y => y.NexusModsUser.NexusModsUserId == user.NexusModsUserId).Select(y => y.Status).FirstOrDefault(),
                Comment = x.ToUsers.Where(y => y.NexusModsUser.NexusModsUserId == user.NexusModsUserId).Select(y => y.Comment).FirstOrDefault(),
            })
            .WithFilter(query.Filters ?? [])
            .WithSort(query.Sortings ?? []);

        var dbQuery = applicationRole == ApplicationRoles.Administrator || applicationRole == ApplicationRoles.Moderator
            ? DbQueryBase(x => true)
            : DbQueryBase(x => x.ToUsers.Any(y => y.NexusModsUser.NexusModsUserId == user.NexusModsUserId) ||
                                                                      x.ModuleInfos.Any(y => moduleIds.Contains(y.Module.ModuleId)) ||
                                                                      x.ModuleInfos.Where(y => y.NexusModsMod != null).Any(y => nexusModsModIds.Contains(y.NexusModsMod!.NexusModsModId)));

        //return await dbQuery.PaginatedGroupedAsync(query.Page, query.PageSize, ct);
        return await dbQuery.PaginatedAsync(query.Page, query.PageSize, ct);
    }
}