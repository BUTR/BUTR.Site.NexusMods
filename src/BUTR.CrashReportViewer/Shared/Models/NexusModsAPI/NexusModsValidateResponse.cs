using System.Text.Json.Serialization;

namespace BUTR.CrashReportViewer.Shared.Models.NexusModsAPI
{
    public class NexusModsValidateResponse
    {
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("is_premium?")]
        public bool IsPremium0 { get; set; }

        [JsonPropertyName("is_supporter?")]
        public bool IsSupporter0 { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("profile_url")]
        public string ProfileUrl { get; set; }

        [JsonPropertyName("is_supporter")]
        public bool IsSupporter { get; set; }

        [JsonPropertyName("is_premium")]
        public bool IsPremium { get; set; }

        [JsonIgnore]
        public string Url => $"https://www.nexusmods.com/users/{UserId}";
    }
}