namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsUserRoleEntity : IEntity
{
    public required int NexusModsUserId { get; init; }

    public required string Role { get; init; }
}