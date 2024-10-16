using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface IUpsertEntityFactory
{
    NexusModsUserEntity GetOrCreateNexusModsUser(NexusModsUserId nexusModsUserId);
    NexusModsUserEntity GetOrCreateNexusModsUserWithName(NexusModsUserId nexusModsUserId, NexusModsUserName nexusModsUserName);
    NexusModsModEntity GetOrCreateNexusModsMod(NexusModsModId nexusModsModId);
    SteamWorkshopModEntity GetOrCreateSteamWorkshopMod(SteamWorkshopModId steamWorkshopModId);
    ModuleEntity GetOrCreateModule(ModuleId moduleId);
    ExceptionTypeEntity GetOrCreateExceptionType(ExceptionTypeId exception);
    Task<bool> SaveCreatedAsync(CancellationToken ct);
}