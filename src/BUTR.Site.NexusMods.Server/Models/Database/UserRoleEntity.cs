namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record UserRoleEntity : IEntity
    {
        public int UserId { get; set; } = default!;

        public string Role { get; set; } = default!;
    }
}