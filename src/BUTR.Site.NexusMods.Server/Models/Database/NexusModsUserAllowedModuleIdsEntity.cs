namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsUserAllowedModuleIdsEntity : IEntity
{
    public required int NexusModsUserId { get; init; }

    public required string[] AllowedModuleIds { get; init; }
}