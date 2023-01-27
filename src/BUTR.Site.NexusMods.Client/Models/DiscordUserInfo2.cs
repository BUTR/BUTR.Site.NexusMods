using BUTR.Site.NexusMods.ServerClient;

namespace BUTR.Site.NexusMods.Client.Models
{
    public sealed record DiscordUserInfo2
    {
        public string Url { get; init; }
        public string Name { get; init; }

        public DiscordUserInfo2(DiscordUserInfo userInfo)
        {
            Url = $"discord://discordapp.com/users/{userInfo.User.Id}";
            Name = $"@{userInfo.User.Username}#{userInfo.User.Discriminator}";
        }
    };
}