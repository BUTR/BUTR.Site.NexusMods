namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record SubModuleInfoTagModel
{
    public required string Key { get; init; }
    public required string[] Value { get; init; }
}