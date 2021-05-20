namespace BUTR.CrashReportViewer.Shared.Models
{
    public record ModModel(string Name, string GameDomain, int ModId)
    {
        public string Url => $"https://nexusmods.com/{GameDomain}/mods/{ModId}";
    }
}