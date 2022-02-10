using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Shared.Models
{
    public record ProfileModel(int UserId, string Name, string Email, string ProfileUrl, bool IsPremium, bool IsSupporter, string Role)
    {
        [JsonIgnore]
        public string? Url => UserId != -1 ? $"https://nexusmods.com/users/{UserId}" : null;
    }
}