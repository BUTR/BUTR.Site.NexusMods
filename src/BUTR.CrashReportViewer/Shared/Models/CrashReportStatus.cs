using System.ComponentModel;

namespace BUTR.CrashReportViewer.Shared.Models
{
    public enum CrashReportStatus
    {
        [Description("New")]
        New,
        [Description("Being Looked At")]
        BeingLookedAt,
        [Description("Not My Fault")]
        NotMyFault,
        [Description("Known Issue")]
        Known,
        [Description("Duplicate")]
        Duplicate,
        [Description("Needs More Info")]
        NeedsMoreInfo,
        [Description("Fixed")]
        Fixed
    }
}