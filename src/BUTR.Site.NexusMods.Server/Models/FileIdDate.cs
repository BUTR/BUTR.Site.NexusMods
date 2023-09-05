using System;
using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Server.Models;

public sealed record FileIdDate(
    [property: JsonPropertyName("filename")] CrashReportFileId FileId, [property: JsonPropertyName("date")] DateTime Date);