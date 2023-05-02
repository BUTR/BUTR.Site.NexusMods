namespace BUTR.Site.NexusMods.Server.Models.Database;

public record NexusModsModEntity : IEntity
{
    public required int NexusModsModId { get; init; }

    public required string Name { get; init; }

    public required int[] UserIds { get; init; }
}