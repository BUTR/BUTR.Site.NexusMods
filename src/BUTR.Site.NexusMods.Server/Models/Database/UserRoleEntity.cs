namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record UserRoleEntity : IEntity
    {
        public required int UserId { get; init; }

        public required string Role { get; init; }
    }
}