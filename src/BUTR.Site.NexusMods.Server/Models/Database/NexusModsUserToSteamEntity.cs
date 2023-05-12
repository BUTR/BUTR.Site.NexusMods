﻿using BUTR.Site.NexusMods.Server.Services;

namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record NexusModsUserToSteamEntity : INexusModsToExternalEntity
    {
        public required int NexusModsUserId { get; init; }
        public required string UserId { get; init; }
    }
}