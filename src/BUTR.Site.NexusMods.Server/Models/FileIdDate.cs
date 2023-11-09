using System;
using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Server.Models;

public sealed record CrashReportFileMetadata(
    [property: JsonPropertyName("file")] CrashReportFileId FileId,
    [property: JsonPropertyName("id")] CrashReportId CrashReportId,
    [property: JsonPropertyName("version")] byte Version,
    [property: JsonPropertyName("date")] DateTime Date);
