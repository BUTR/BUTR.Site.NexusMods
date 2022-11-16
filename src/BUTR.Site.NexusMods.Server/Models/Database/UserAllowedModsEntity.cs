namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record UserAllowedModsEntity : IEntity
    {
        public required int UserId { get; init; }

        public required string[] AllowedModIds { get; init; }
    }
}