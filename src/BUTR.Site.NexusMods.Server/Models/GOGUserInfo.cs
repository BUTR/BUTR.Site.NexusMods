using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Server.Models;

public sealed record GOGUserInfo(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("username")] string Username);