using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Models
{
    public record SteamUserTokens(string UserId, Dictionary<string, string> Data);
}