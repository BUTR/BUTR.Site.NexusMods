using BUTR.Site.NexusMods.ServerClient;

namespace BUTR.Site.NexusMods.Client.Models
{
    public sealed record SteamUserInfo2
    {
        public string Url { get; init; }
        public string Name { get; init; }
        public bool NeedsRelink { get; init; }

        public SteamUserInfo2(SteamUserInfo? userInfo)
        {
            if (userInfo is null)
            {
                NeedsRelink = true;
                Url = string.Empty;
                Name = string.Empty;
            }
            else
            {
                Url = $"https://steamcommunity.com/profiles/{userInfo.Id}";
                Name = userInfo.Username;
            }
        }
    };
}