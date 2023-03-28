namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed class TopExceptionsTypeEntity : IEntity
{
    public required string Type { get; init; }
    public required int Count { get; init; }
}