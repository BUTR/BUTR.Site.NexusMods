namespace BUTR.CrashReportViewer.Shared.Models
{
    public record ProfileModel(int UserId, string Name, string Email, string ProfileUrl, bool IsPremium, bool IsSupporter)
    {
        public string Url => $"https://nexusmods.com/users/{UserId}";
    }
}