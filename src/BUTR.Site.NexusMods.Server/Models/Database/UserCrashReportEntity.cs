using BUTR.Site.NexusMods.Server.Models.API;

namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record UserCrashReportEntity : IEntity
    {
        public int UserId { get; set; } = default!;

        public CrashReportEntity CrashReport { get; set; } = default!;

        public CrashReportStatus Status { get; set; } = default!;

        public string Comment { get; set; } = default!;
    }
}