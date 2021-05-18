namespace BUTR.CrashReportViewer.Shared.Models
{
    public class ModModel
    {
        public string Name { get; set; }
        public string GameDomain { get; set; }
        public int ModId { get; set; }

        public string Url => $"https://nexusmods.com/{GameDomain}/mods/{ModId}";
    }

    public class LinkModModel
    {
        public string ModUrl { get; set; }
    }
}