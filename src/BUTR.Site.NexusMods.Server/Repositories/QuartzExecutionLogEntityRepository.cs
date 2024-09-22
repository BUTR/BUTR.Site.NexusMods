using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

[ScopedService<IQuartzExecutionLogEntityRepositoryWrite, IQuartzExecutionLogEntityRepositoryRead>]
internal class QuartzExecutionLogEntityRepository : Repository<QuartzExecutionLogEntity>, IQuartzExecutionLogEntityRepositoryWrite
{
    public QuartzExecutionLogEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }

    public async Task MarkIncompleteAsync(CancellationToken ct) => await _dbContext.QuartzExecutionLogs
        .Where(x => !x.IsSuccess.HasValue)
        .ExecuteUpdateAsync(calls => calls
            .SetProperty(x => x.IsSuccess, false)
            .SetProperty(x => x.ErrorMessage, "Incomplete execution.")
            .SetProperty(x => x.JobRunTime, TimeSpan.Zero), ct);
}