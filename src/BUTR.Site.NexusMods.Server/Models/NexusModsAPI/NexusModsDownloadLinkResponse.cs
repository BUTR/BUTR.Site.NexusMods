using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Server.Models.NexusModsAPI;

public record NexusModsDownloadLinkResponse(
    [property: JsonPropertyName("URI")] string Url
);