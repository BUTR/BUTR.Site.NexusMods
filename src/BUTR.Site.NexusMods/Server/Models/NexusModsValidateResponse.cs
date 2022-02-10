using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Server.Models
{
    public record NexusModsValidateResponse
    {
        [JsonPropertyName("user_id")]
        public uint UserId { get; set; } = default!;

        [JsonPropertyName("key")]
        public string Key { get; set; } = default!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;

        //[JsonPropertyName("is_premium?")]
        //public bool IsPremium0 { get; set; } = default!;

        //[JsonPropertyName("is_supporter?")]
        //public bool IsSupporter0 { get; set; } = default!;

        [JsonPropertyName("email")]
        public string Email { get; set; } = default!;

        [JsonPropertyName("profile_url")]
        public string ProfileUrl { get; set; } = default!;

        [JsonPropertyName("is_supporter")]
        public bool IsSupporter { get; set; } = default!;

        [JsonPropertyName("is_premium")]
        public bool IsPremium { get; set; } = default!;
    }
}