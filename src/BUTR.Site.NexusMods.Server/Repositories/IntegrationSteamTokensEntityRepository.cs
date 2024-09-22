using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Repositories;

[ScopedService<IIntegrationSteamTokensEntityRepositoryWrite, IIntegrationSteamTokensEntityRepositoryRead>]
internal class IntegrationSteamTokensEntityRepository : Repository<IntegrationSteamTokensEntity>, IIntegrationSteamTokensEntityRepositoryWrite
{
    protected override IQueryable<IntegrationSteamTokensEntity> InternalQuery => base.InternalQuery
        .Include(x => x.NexusModsUser).ThenInclude(x => x.Name);

    public IntegrationSteamTokensEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }
}