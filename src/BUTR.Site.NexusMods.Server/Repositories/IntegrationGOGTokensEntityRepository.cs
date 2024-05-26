using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface IIntegrationGOGTokensEntityRepositoryRead : IRepositoryRead<IntegrationGOGTokensEntity>;
public interface IIntegrationGOGTokensEntityRepositoryWrite : IRepositoryWrite<IntegrationGOGTokensEntity>, IIntegrationGOGTokensEntityRepositoryRead;

[ScopedService<IIntegrationGOGTokensEntityRepositoryWrite, IIntegrationGOGTokensEntityRepositoryRead>]
internal class IntegrationGOGTokensEntityRepository : Repository<IntegrationGOGTokensEntity>, IIntegrationGOGTokensEntityRepositoryWrite
{
    protected override IQueryable<IntegrationGOGTokensEntity> InternalQuery => base.InternalQuery
        .Include(x => x.NexusModsUser).ThenInclude(x => x.Name)
        .Include(x => x.UserToGOG);

    public IntegrationGOGTokensEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }
}