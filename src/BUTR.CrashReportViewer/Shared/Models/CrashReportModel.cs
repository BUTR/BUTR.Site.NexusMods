using System;

namespace BUTR.CrashReportViewer.Shared.Models
{
    public class CrashReportModel
    {
        public Guid Id { get; set; } = default!;

        public DateTime Date { get; set; } = default!;

        public CrashReportStatus Status { get; set; } = default!;

        public string Comment { get; set; } = default!;
    }
}