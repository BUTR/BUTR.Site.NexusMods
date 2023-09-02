using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Server.Models;

public sealed record SteamUserInfo(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("username")] string Username);