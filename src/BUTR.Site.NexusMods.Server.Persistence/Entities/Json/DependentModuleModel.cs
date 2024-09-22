namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record DependentModuleModel
{
    public required string Id { get; init; }
    public required string Type { get; init; }
    public required bool IsOptional { get; init; }
    public required string? Version { get; init; }
    public required ApplicationVersionRangeModel? VersionRange { get; init; }
}