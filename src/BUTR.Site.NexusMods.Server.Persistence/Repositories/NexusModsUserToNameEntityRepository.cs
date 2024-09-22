using BUTR.Site.NexusMods.Server.Models.Database;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface INexusModsUserToNameEntityRepositoryRead : IRepositoryRead<NexusModsUserToNameEntity>;
public interface INexusModsUserToNameEntityRepositoryWrite : IRepositoryWrite<NexusModsUserToNameEntity>, INexusModsUserToNameEntityRepositoryRead;