using BUTR.Site.NexusMods.Server.Models.API;

namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record UserCrashReportEntity : IEntity
    {
        public required int UserId { get; init; }

        public required CrashReportEntity CrashReport { get; init; }

        public required CrashReportStatus Status { get; init; }

        public required string Comment { get; init; }
    }
}