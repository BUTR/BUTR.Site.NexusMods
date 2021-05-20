namespace BUTR.CrashReportViewer.Server.Models.Contexts
{
    public class ModTable
    {
        public string Name { get; set; } = default!;

        public string GameDomain { get; set; } = default!;

        public int ModId { get; set; } = default!;

        public int[] UserIds { get; set; } = default!;
    }
}