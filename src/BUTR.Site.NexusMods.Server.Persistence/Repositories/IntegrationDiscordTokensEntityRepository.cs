using BUTR.Site.NexusMods.Server.Models.Database;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface IIntegrationDiscordTokensEntityRepositoryRead : IRepositoryRead<IntegrationDiscordTokensEntity>;
public interface IIntegrationDiscordTokensEntityRepositoryWrite : IRepositoryWrite<IntegrationDiscordTokensEntity>, IIntegrationDiscordTokensEntityRepositoryRead;