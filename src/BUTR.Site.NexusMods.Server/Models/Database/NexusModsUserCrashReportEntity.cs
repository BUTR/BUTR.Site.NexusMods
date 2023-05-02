using BUTR.Site.NexusMods.Server.Models.API;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsUserCrashReportEntity : IEntity
{
    public required int NexusModsUserId { get; init; }

    public required CrashReportEntity CrashReport { get; init; }

    public required CrashReportStatus Status { get; init; }

    public required string Comment { get; init; }
}