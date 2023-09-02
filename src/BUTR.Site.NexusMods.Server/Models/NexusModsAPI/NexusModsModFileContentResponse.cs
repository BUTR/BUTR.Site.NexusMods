using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Server.Models.NexusModsAPI;

public sealed record NexusModsModFileContentResponse(
    [property: JsonPropertyName("children")]
    IReadOnlyList<NexusModsModFileContentResponse.ContentEntry> Children
)
{
    public sealed record ContentEntry(
        [property: JsonPropertyName("path")] string Path,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("children")] IReadOnlyList<ContentEntry> Children
    );
}