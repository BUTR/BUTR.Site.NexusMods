using BUTR.Site.NexusMods.ServerClient;

namespace BUTR.Site.NexusMods.Client.Models
{
    public sealed record GitHubUserInfo2
    {
        public string Url { get; init; }
        public string Name { get; init; }
        public bool NeedsRelink { get; init; }

        public GitHubUserInfo2(GitHubUserInfo? userInfo)
        {
            if (userInfo is null)
            {
                NeedsRelink = true;
                Url = string.Empty;
                Name = string.Empty;
            }
            else
            {
                Url = $"https://github.com/{userInfo.Login}";
                Name = userInfo.Login;
            }
        }
    };
}