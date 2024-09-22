using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using EFCore.BulkExtensions;

using Microsoft.EntityFrameworkCore;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

[ScopedService<IStatisticsTopExceptionsTypeEntityRepositoryWrite, IStatisticsTopExceptionsTypeEntityRepositoryRead>]
internal class StatisticsTopExceptionsTypeEntityRepository : Repository<StatisticsTopExceptionsTypeEntity>, IStatisticsTopExceptionsTypeEntityRepositoryWrite
{
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public StatisticsTopExceptionsTypeEntityRepository(IAppDbContextProvider appDbContextProvider, ITenantContextAccessor tenantContextAccessor) : base(appDbContextProvider.Get())
    {
        _tenantContextAccessor = tenantContextAccessor;
    }

    protected override IQueryable<StatisticsTopExceptionsTypeEntity> InternalQuery => base.InternalQuery
        .Include(x => x.ExceptionType);

    public async Task CalculateAsync(CancellationToken ct)
    {
        if (_dbContext is not AppDbContextWrite dbContextWrite)
            throw AppDbContextRead.WriteNotSupported();

        var tenant = _tenantContextAccessor.Current;
        var upsertEntityFactory = dbContextWrite.GetEntityFactory();
        var statistics = dbContextWrite.ExceptionTypes.Select(x => new StatisticsTopExceptionsTypeEntity
        {
            TenantId = tenant,
            ExceptionTypeId = x.ExceptionTypeId,
            ExceptionType = upsertEntityFactory.GetOrCreateExceptionType(x.ExceptionTypeId),
            ExceptionCount = x.ToCrashReports.Count
        }).ToList();

        await dbContextWrite.StatisticsTopExceptionsTypes.Where(x => true).ExecuteDeleteAsync(cancellationToken: ct);
        await dbContextWrite.BulkInsertOrUpdateAsync(statistics, cancellationToken: ct);
    }
}