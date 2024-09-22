using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Repositories;

[ScopedService<IIntegrationGitHubTokensEntityRepositoryWrite, IIntegrationGitHubTokensEntityRepositoryRead>]
internal class IntegrationGitHubTokensEntityRepository : Repository<IntegrationGitHubTokensEntity>, IIntegrationGitHubTokensEntityRepositoryWrite
{
    protected override IQueryable<IntegrationGitHubTokensEntity> InternalQuery => base.InternalQuery
        .Include(x => x.NexusModsUser).ThenInclude(x => x.Name)
        .Include(x => x.UserToGitHub);

    public IntegrationGitHubTokensEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }
}