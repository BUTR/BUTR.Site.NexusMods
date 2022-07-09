using BUTR.Site.NexusMods.Server.Models.API;

namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record UserCrashReportTableEntry
    {
        public int UserId { get; set; } = default!;

        public CrashReportTableEntry? CrashReport { get; set; } = default!;

        public CrashReportStatus Status { get; set; } = default!;

        public string Comment { get; set; } = default!;
    }
}