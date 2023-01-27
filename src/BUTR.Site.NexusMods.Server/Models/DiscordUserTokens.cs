using System;

namespace BUTR.Site.NexusMods.Server.Models
{
    public record DiscordUserTokens(string UserId, string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);
}