using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Server.Models
{
    public record NexusModsDownloadLinkResponse(
        [property: JsonPropertyName("URI")] string Url
    );
}