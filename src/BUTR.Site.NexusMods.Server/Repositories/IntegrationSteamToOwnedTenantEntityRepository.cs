using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Repositories;

[ScopedService<IIntegrationSteamToOwnedTenantEntityRepositoryWrite, IIntegrationSteamToOwnedTenantEntityRepositoryRead>]
internal class IntegrationSteamToOwnedTenantEntityRepository : Repository<IntegrationSteamToOwnedTenantEntity>, IIntegrationSteamToOwnedTenantEntityRepositoryWrite
{
    protected override IQueryable<IntegrationSteamToOwnedTenantEntity> InternalQuery => base.InternalQuery;

    public IntegrationSteamToOwnedTenantEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }
}