namespace BUTR.Site.NexusMods.Server.Models.API
{
    public sealed record ProfileModel
    {
        public required int UserId { get; init; }
        public required string Name { get; init; }
        public required string Email { get; init; }
        public required string ProfileUrl { get; init; }
        public required bool IsPremium { get; init; }
        public required bool IsSupporter { get; init; }
        public required string Role { get; init; }

        public required string? DiscordUserId { get; init; }
    }
}