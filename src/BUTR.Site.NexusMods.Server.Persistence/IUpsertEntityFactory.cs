using System.Threading;
using System.Threading.Tasks;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface IUpsertEntityFactory
{
    NexusModsUserEntity GetOrCreateNexusModsUser(NexusModsUserId nexusModsUserId);
    NexusModsUserEntity GetOrCreateNexusModsUserWithName(NexusModsUserId nexusModsUserId, NexusModsUserName nexusModsUserName);
    NexusModsModEntity GetOrCreateNexusModsMod(NexusModsModId nexusModsModId);
    ModuleEntity GetOrCreateModule(ModuleId moduleId);
    ExceptionTypeEntity GetOrCreateExceptionType(ExceptionTypeId exception);
    Task<bool> SaveCreatedAsync(CancellationToken ct);
}