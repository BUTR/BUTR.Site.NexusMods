using BUTR.Site.NexusMods.Shared.Models;

namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public record UserCrashReportTableEntry
    {
        public int UserId { get; set; } = default!;

        public CrashReportTableEntry? CrashReport { get; set; } = default!;

        public CrashReportStatus Status { get; set; } = default!;

        public string Comment { get; set; } = default!;
    }
}