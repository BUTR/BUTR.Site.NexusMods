using System;

namespace BUTR.CrashReportViewer.Shared.Models
{
    public record CrashReportModel(Guid Id, string Exception, DateTime Date, string Url)
    {
        public CrashReportStatus Status { get; set; } = default!;
        public string Comment { get; set; } = default!;
    }
}