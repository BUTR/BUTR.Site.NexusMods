using System;
using System.Collections.Generic;

namespace BUTR.CrashReportViewer.Server.Models.Database
{
    public record CrashReportTableEntry
    {
        public Guid Id { get; set; } = default!;

        public string Exception { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = default!;

        public int[] ModIds { get; set; } = default!;

        public string Url { get; set; } = default!;

        public List<UserCrashReportTableEntry> UserCrashReports { get; set; } = default!;
    }
}