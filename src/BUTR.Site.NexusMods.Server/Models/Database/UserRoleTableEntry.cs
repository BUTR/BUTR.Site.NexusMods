namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record UserRoleTableEntry
    {
        public int UserId { get; set; } = default!;

        public string Role { get; set; } = default!;
    }
}