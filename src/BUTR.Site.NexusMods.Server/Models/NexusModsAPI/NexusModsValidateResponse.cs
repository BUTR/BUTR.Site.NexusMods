using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Server.Models.NexusModsAPI;

public sealed record NexusModsValidateResponse(
    [property: JsonPropertyName("user_id")] NexusModsUserId UserId,
    [property: JsonPropertyName("key")] NexusModsApiKey Key,
    [property: JsonPropertyName("name")] NexusModsUserName Name,
    [property: JsonPropertyName("email")] NexusModsUserEMail Email,
    [property: JsonPropertyName("profile_url")] string ProfileUrl,
    [property: JsonPropertyName("is_supporter")] bool IsSupporter,
    [property: JsonPropertyName("is_premium")] bool IsPremium
);