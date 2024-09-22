using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Repositories;

[ScopedService<IIntegrationDiscordTokensEntityRepositoryWrite, IIntegrationDiscordTokensEntityRepositoryRead>]
internal class IntegrationDiscordTokensEntityRepository : Repository<IntegrationDiscordTokensEntity>, IIntegrationDiscordTokensEntityRepositoryWrite
{
    protected override IQueryable<IntegrationDiscordTokensEntity> InternalQuery => base.InternalQuery
        .Include(x => x.NexusModsUser).ThenInclude(x => x.Name)
        .Include(x => x.UserToDiscord);

    public IntegrationDiscordTokensEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }
}