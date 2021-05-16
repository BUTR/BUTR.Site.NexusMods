namespace BUTR.CrashReportViewer.Shared.Models.Contexts
{
    public class UserCrashReportTable
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public CrashReportTable CrashReport { get; set; }

        public CrashReportStatus Status { get; set; }

        public string Comment { get; set; }
    }
}