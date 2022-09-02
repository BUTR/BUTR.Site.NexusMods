namespace BUTR.Site.NexusMods.Server.Models.API
{
    public sealed record ProfileModel
    {
        public int UserId { get; init; }
        public string Name { get; init; }
        public string Email { get; init; }
        public string ProfileUrl { get; init; }
        public bool IsPremium { get; init; }
        public bool IsSupporter { get; init; }
        public string Role { get; init; }
    }
}