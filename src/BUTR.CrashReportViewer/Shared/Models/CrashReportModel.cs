using System;

namespace BUTR.CrashReportViewer.Shared.Models
{
    public record CrashReportModel(Guid Id, DateTime Date, CrashReportStatus Status)
    {
        public string Comment { get; set; } = default!;
    }
}