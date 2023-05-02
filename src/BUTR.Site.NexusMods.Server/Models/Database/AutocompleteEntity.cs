namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record AutocompleteEntity : IEntity
{
    public required string Type { get; init; }
    public required string[] Values { get; init; }
}