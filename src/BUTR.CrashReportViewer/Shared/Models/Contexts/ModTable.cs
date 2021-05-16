namespace BUTR.CrashReportViewer.Shared.Models.Contexts
{
    public class ModTable
    {
        public string GameDomain { get; set; }

        public int ModId { get; set; }

        public int[] UserIds { get; set; }
    }
}