using BUTR.Site.NexusMods.Server.Models.Database;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface INexusModsModToNameEntityRepositoryRead : IRepositoryRead<NexusModsModToNameEntity>;
public interface INexusModsModToNameEntityRepositoryWrite : IRepositoryWrite<NexusModsModToNameEntity>, INexusModsModToNameEntityRepositoryRead;