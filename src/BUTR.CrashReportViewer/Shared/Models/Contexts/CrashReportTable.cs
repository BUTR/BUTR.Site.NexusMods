using System;
using System.Collections.Generic;

namespace BUTR.CrashReportViewer.Shared.Models.Contexts
{
    public class CrashReportTable
    {
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public int[] ModIds { get; set; }

        public List<UserCrashReportTable> UserCrashReports { get; set; }
    }
}