using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface INexusModsUserToIntegrationGitHubEntityRepositoryRead : IRepositoryRead<NexusModsUserToIntegrationGitHubEntity>;
public interface INexusModsUserToIntegrationGitHubEntityRepositoryWrite : IRepositoryWrite<NexusModsUserToIntegrationGitHubEntity>, INexusModsUserToIntegrationGitHubEntityRepositoryRead;

[ScopedService<INexusModsUserToIntegrationGitHubEntityRepositoryWrite, INexusModsUserToIntegrationGitHubEntityRepositoryRead>]
internal class NexusModsUserToIntegrationGitHubEntityRepository : Repository<NexusModsUserToIntegrationGitHubEntity>, INexusModsUserToIntegrationGitHubEntityRepositoryWrite
{
    protected override IQueryable<NexusModsUserToIntegrationGitHubEntity> InternalQuery => base.InternalQuery
        .Include(x => x.NexusModsUser)
        .Include(x => x.ToTokens);

    public NexusModsUserToIntegrationGitHubEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }
}