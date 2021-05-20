using System;
using System.Collections.Generic;

namespace BUTR.CrashReportViewer.Server.Models.Contexts
{
    public class CrashReportTable
    {
        public Guid Id { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = default!;

        public int[] ModIds { get; set; } = default!;

        public List<UserCrashReportTable> UserCrashReports { get; set; } = default!;
    }
}