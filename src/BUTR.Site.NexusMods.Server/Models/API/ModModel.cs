using System.Collections.Immutable;

namespace BUTR.Site.NexusMods.Server.Models.API
{
    public sealed record ModModel(string Name, int ModId, ImmutableArray<int> AllowedUserIds, ImmutableArray<int> ManuallyLinkedUserIds, ImmutableArray<string> ManuallyLinkedModuleIds, ImmutableArray<string> KnownModuleIds);
}