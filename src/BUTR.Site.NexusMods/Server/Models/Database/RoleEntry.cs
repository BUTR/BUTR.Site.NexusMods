namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public record RoleEntry
    {
        public uint UserId { get; set; } = default!;

        public string Role { get; set; } = default!;
    }
}