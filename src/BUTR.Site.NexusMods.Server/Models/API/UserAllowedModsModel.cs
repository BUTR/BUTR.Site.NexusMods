using System.Collections.Generic;
using System.Collections.Immutable;

namespace BUTR.Site.NexusMods.Server.Models.API
{
    public sealed record UserAllowedModsModel(int UserId, ImmutableArray<string> AllowedModIds);
}