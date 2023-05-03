namespace BUTR.Site.NexusMods.Server.Models.API;

public sealed record ModListModel
{
    public required string Name { get; init; }
    public required string Content { get; init; }
}