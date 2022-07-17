using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Server.Models
{
    public record NexusModsDownloadLinkResponse(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("short_name")] string ShortName,
        [property: JsonPropertyName("URI")] string URI
    );
}