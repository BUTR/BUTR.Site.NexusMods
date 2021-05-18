namespace BUTR.CrashReportViewer.Shared.Models
{
    public class ModModel
    {
        public string Name { get; set; } = default!;
        public string GameDomain { get; set; } = default!;
        public int ModId { get; set; } = default!;

        public string Url => $"https://nexusmods.com/{GameDomain}/mods/{ModId}";
    }
}