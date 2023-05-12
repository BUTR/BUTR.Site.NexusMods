using BUTR.Site.NexusMods.ServerClient;

namespace BUTR.Site.NexusMods.Client.Models
{
    public sealed record GOGUserInfo2
    {
        public string Url { get; init; }
        public string Name { get; init; }

        public GOGUserInfo2(GOGUserInfo userInfo)
        {
            Url = $"https://www.gog.com/u/{userInfo.Username}";
            Name = userInfo.Username;
        }
    };
}