using System.Collections.Immutable;

namespace BUTR.Site.NexusMods.Server.Models.API;

public sealed record NexusModsModModel(
    int NexusModsModId,
    string Name,
    ImmutableArray<int> AllowedNexusModsUserIds,
    ImmutableArray<int> ManuallyLinkedNexusModsUserIds,
    ImmutableArray<string> ManuallyLinkedModuleIds,
    ImmutableArray<string> KnownModuleIds);