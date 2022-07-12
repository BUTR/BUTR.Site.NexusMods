namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record UserAllowedModsEntity : IEntity
    {
        public int UserId { get; set; } = default!;

        public string[] AllowedModIds { get; set; } = default!;
    }
}