using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Server.Models
{
    public record NexusModsModFilesResponse(
        [property: JsonPropertyName("files")] IReadOnlyList<NexusModsModFilesResponse.File> Files,
        [property: JsonPropertyName("file_updates")] IReadOnlyList<NexusModsModFilesResponse.FileUpdate> FileUpdates
    )
    {
        public record File(
            [property: JsonPropertyName("file_id")] int Id,
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("version")] string Version,
            [property: JsonPropertyName("file_name")] string FileName,
            [property: JsonPropertyName("uploaded_timestamp")] long UploadedTimestamp,
            [property: JsonPropertyName("mod_version")] string ModVersion,
            [property: JsonPropertyName("external_virus_scan_url")] string ExternalVirusScanUrl,
            [property: JsonPropertyName("content_preview_link")] string ContentPreviewUrl
        );

        public record FileUpdate(
            [property: JsonPropertyName("old_file_id")] int OldId,
            [property: JsonPropertyName("new_file_id")] int NewId,
            [property: JsonPropertyName("old_file_name")] string OldName,
            [property: JsonPropertyName("new_file_name")] string NewName,
            [property: JsonPropertyName("uploaded_timestamp")] long UploadedTimestamp
        );
    }
}