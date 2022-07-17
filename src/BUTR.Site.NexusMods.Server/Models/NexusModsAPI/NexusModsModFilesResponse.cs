using System;
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
            [property: JsonPropertyName("id")] IReadOnlyList<int> Id,
            [property: JsonPropertyName("uid")] long Uid,
            [property: JsonPropertyName("file_id")] int FileId,
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("version")] string Version,
            [property: JsonPropertyName("category_id")] int CategoryId,
            [property: JsonPropertyName("category_name")] string CategoryName,
            [property: JsonPropertyName("is_primary")] bool IsPrimary,
            [property: JsonPropertyName("size")] int Size,
            [property: JsonPropertyName("file_name")] string FileName,
            [property: JsonPropertyName("uploaded_timestamp")] int UploadedTimestamp,
            [property: JsonPropertyName("uploaded_time")] DateTime UploadedTime,
            [property: JsonPropertyName("mod_version")] string ModVersion,
            [property: JsonPropertyName("external_virus_scan_url")] object ExternalVirusScanUrl,
            [property: JsonPropertyName("description")] string Description,
            [property: JsonPropertyName("size_kb")] int SizeKb,
            [property: JsonPropertyName("size_in_bytes")] long SizeInBytes,
            [property: JsonPropertyName("changelog_html")] string ChangelogHtml,
            [property: JsonPropertyName("content_preview_link")] string ContentPreviewLink
        );

        public record FileUpdate(
            [property: JsonPropertyName("old_file_id")] int OldFileId,
            [property: JsonPropertyName("new_file_id")] int NewFileId,
            [property: JsonPropertyName("old_file_name")] string OldFileName,
            [property: JsonPropertyName("new_file_name")] string NewFileName,
            [property: JsonPropertyName("uploaded_timestamp")] int UploadedTimestamp,
            [property: JsonPropertyName("uploaded_time")] DateTime UploadedTime
        );
    }
}