using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Shared.Models
{
    public record CrashReportModel(Guid Id, string Exception, DateTime Date, string Url)
    {
        [JsonIgnore]
        public string? Type => Exception.Split(Environment.NewLine).FirstOrDefault(l => l.Contains("Type:"))?.Split("Type:").Skip(1).FirstOrDefault();

        [JsonIgnore]
        public string ExceptionHtml => Exception.Replace("\r", "<br/>").Replace("\r\n", "<br/>");

        public CrashReportStatus Status { get; set; } = default!;
        public string Comment { get; set; } = default!;
    }
}