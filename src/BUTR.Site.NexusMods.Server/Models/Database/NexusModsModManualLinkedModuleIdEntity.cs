namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsModManualLinkedModuleIdEntity : IEntity
{
    public required int NexusModsModId { get; init; }
    
    public required string ModuleId { get; init; }
}