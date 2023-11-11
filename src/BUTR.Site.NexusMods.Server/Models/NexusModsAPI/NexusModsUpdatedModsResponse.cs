using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Server.Models.NexusModsAPI;

public sealed record NexusModsUpdatedModsResponse(
    [property: JsonPropertyName("mod_id")] NexusModsModId Id,
    [property: JsonPropertyName("latest_file_update")] long LatestFileUpdateTimestamp,
    [property: JsonPropertyName("latest_mod_activity")] long LatestModActivityTimestamp
);