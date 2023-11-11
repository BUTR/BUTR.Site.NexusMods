using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Server.Models.NexusModsAPI;

public sealed record NexusModsModFilesResponse(
    [property: JsonPropertyName("files")] IReadOnlyList<NexusModsModFilesResponse.File> Files,
    [property: JsonPropertyName("file_updates")] IReadOnlyList<NexusModsModFilesResponse.FileUpdate> FileUpdates
)
{
    public sealed record File(
        [property: JsonPropertyName("file_id")] NexusModsFileId? FileId,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("version")] string Version,
        [property: JsonPropertyName("category_id")] int? CategoryId,
        [property: JsonPropertyName("category_name")] string? CategoryName,
        [property: JsonPropertyName("is_primary")] bool? IsPrimary,
        [property: JsonPropertyName("size")] long? Size,
        [property: JsonPropertyName("file_name")] string FileName,
        [property: JsonPropertyName("uploaded_timestamp")] long? UploadedTimestamp,
        [property: JsonPropertyName("mod_version")] string ModVersion,
        [property: JsonPropertyName("external_virus_scan_url")] string ExternalVirusScanUrl,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("size_in_bytes")] long? SizeInBytes,
        [property: JsonPropertyName("changelog_html")] string ChangelogHtml,
        [property: JsonPropertyName("content_preview_link")] string ContentPreviewLink
    );

    public sealed record FileUpdate(
        [property: JsonPropertyName("old_file_id")] NexusModsFileId OldId,
        [property: JsonPropertyName("new_file_id")] NexusModsFileId NewId,
        [property: JsonPropertyName("old_file_name")] string OldName,
        [property: JsonPropertyName("new_file_name")] string NewName,
        [property: JsonPropertyName("uploaded_timestamp")] long UploadedTimestamp
    );
}