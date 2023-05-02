using System.Collections.Immutable;

namespace BUTR.Site.NexusMods.Server.Models.API
{
    public sealed record UserAllowedModuleIdsModel(int UserId, ImmutableArray<string> AllowedModuleIds);
}