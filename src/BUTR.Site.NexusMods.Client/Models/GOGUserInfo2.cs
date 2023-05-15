using BUTR.Site.NexusMods.ServerClient;

namespace BUTR.Site.NexusMods.Client.Models
{
    public sealed record GOGUserInfo2
    {
        public string Url { get; init; }
        public string Name { get; init; }
        public bool NeedsRelink { get; init; }

        public GOGUserInfo2(GOGUserInfo? userInfo)
        {
            if (userInfo is null)
            {
                NeedsRelink = true;
                Url = string.Empty;
                Name = string.Empty;
            }
            else
            {
                Url = $"https://www.gog.com/u/{userInfo.Username}";
                Name = userInfo.Username;
            }
        }
    };
}