using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Server.Models.NexusModsAPI;

public sealed record NexusModsValidateResponse(
    [property: JsonPropertyName("user_id")] uint UserId,
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("profile_url")] string ProfileUrl,
    [property: JsonPropertyName("is_supporter")] bool IsSupporter,
    [property: JsonPropertyName("is_premium")] bool IsPremium
);