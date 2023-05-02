namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record CrashReportFileEntity : IEntity
{
    public required string Filename { get; init; }

    public required CrashReportEntity CrashReport { get; init; }
}