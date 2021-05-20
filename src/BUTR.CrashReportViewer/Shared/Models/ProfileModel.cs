namespace BUTR.CrashReportViewer.Shared.Models
{
    public record ProfileModel(int UserId, string Name, string Email, string ProfileUrl, bool IsPremium, bool IsSupporter)
    {
        public string Url => $"https://www.nexusmods.com/users/{UserId}";
    }
}