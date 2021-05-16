using System;

namespace BUTR.CrashReportViewer.Shared.Models
{
    public class CrashReportModel
    {
        public Guid Id { get; set; }

        public DateTime Date { get; set; }

        public CrashReportStatus Status { get; set; }

        public string Comment { get; set; }
    }
}