using System.Collections.Immutable;

namespace BUTR.Site.NexusMods.Server.Models.API
{
    public sealed record UserAllowedModsModel(int ModId, ImmutableArray<int> AllowedUserIds);
}