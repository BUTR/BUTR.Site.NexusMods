using BUTR.Site.NexusMods.Server.Services;

using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record SteamLinkedRoleTokensEntity : IExternalEntity
    {
        public required string UserId { get; init; }

        public required Dictionary<string, string> Data { get; init; }
    }
}