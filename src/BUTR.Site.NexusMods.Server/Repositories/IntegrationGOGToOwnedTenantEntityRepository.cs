using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Repositories;

[ScopedService<IIntegrationGOGToOwnedTenantEntityRepositoryWrite, IIntegrationGOGToOwnedTenantEntityRepositoryRead>]
internal class IntegrationGOGToOwnedTenantEntityRepository : Repository<IntegrationGOGToOwnedTenantEntity>, IIntegrationGOGToOwnedTenantEntityRepositoryWrite
{
    protected override IQueryable<IntegrationGOGToOwnedTenantEntity> InternalQuery => base.InternalQuery;

    public IntegrationGOGToOwnedTenantEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }
}