using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Repositories;

[ScopedService<INexusModsUserToIntegrationDiscordEntityRepositoryWrite, INexusModsUserToIntegrationDiscordEntityRepositoryRead>]
internal class NexusModsUserToIntegrationDiscordEntityRepository : Repository<NexusModsUserToIntegrationDiscordEntity>, INexusModsUserToIntegrationDiscordEntityRepositoryWrite
{
    protected override IQueryable<NexusModsUserToIntegrationDiscordEntity> InternalQuery => base.InternalQuery
        .Include(x => x.NexusModsUser)
        .Include(x => x.ToTokens);

    public NexusModsUserToIntegrationDiscordEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }
}