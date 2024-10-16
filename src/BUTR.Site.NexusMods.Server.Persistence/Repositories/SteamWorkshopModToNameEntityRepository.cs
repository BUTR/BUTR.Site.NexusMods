using BUTR.Site.NexusMods.Server.Models.Database;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface ISteamWorkshopModToNameEntityRepositoryRead : IRepositoryRead<SteamWorkshopModToNameEntity>;
public interface ISteamWorkshopModToNameEntityRepositoryWrite : IRepositoryWrite<SteamWorkshopModToNameEntity>, ISteamWorkshopModToNameEntityRepositoryRead;