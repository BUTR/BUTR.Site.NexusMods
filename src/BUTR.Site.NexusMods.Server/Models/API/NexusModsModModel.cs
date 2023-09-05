using System.Collections.Immutable;

namespace BUTR.Site.NexusMods.Server.Models.API;

public sealed record NexusModsModModel(
    NexusModsModId NexusModsModId,
    string Name,
    ImmutableArray<NexusModsUserId> AllowedNexusModsUserIds,
    ImmutableArray<NexusModsUserId> ManuallyLinkedNexusModsUserIds,
    ImmutableArray<ModuleId> ManuallyLinkedModuleIds,
    ImmutableArray<ModuleId> KnownModuleIds);