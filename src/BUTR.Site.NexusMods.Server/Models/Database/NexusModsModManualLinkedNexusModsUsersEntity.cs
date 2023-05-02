namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsModManualLinkedNexusModsUsersEntity : IEntity
{
    public required int NexusModsModId { get; init; }
    
    public required int[] AllowedNexusModsUserIds { get; init; }
}