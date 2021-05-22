using BUTR.CrashReportViewer.Shared.Models;

namespace BUTR.CrashReportViewer.Server.Models.Contexts
{
    public class UserCrashReportTable
    {
        public int UserId { get; set; } = default!;

        public CrashReportTable CrashReport { get; set; } = default!;

        public CrashReportStatus Status { get; set; } = default!;

        public string Comment { get; set; } = default!;
    }
}