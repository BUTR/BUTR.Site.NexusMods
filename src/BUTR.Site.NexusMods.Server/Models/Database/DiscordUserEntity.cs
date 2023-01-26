using System;

namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record DiscordUserEntity : IEntity
    {
        public required int UserId { get; init; }

        public required string AccessToken { get; init; }
        public required string RefreshToken { get; init; }
        public required DateTimeOffset ExpiresAt { get; init; }
    }
}