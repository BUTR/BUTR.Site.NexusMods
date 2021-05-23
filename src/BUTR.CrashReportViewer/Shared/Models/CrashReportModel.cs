using System;
using System.Linq;

namespace BUTR.CrashReportViewer.Shared.Models
{
    public record CrashReportModel(Guid Id, string Exception, DateTime Date, string Url)
    {
        public CrashReportStatus Status { get; set; } = default!;
        public string Comment { get; set; } = default!;

        public string? Type => Exception.Split(Environment.NewLine)
            .FirstOrDefault(l => l.Contains("Type:"))?.Split("Type:").Skip(1).FirstOrDefault();

        public string ExceptionHtml => Exception.Replace("\r", "<br/>").Replace("\r\n", "<br/>");
    }
}