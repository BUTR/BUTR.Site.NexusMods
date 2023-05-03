using System;
using System.Collections.Immutable;

namespace BUTR.Site.NexusMods.Server.Models.API;

public sealed record CrashReportModel
{
    public required Guid Id { get; init; }
    public required int Version { get; init; }
    public required string GameVersion { get; init; }
    public required string Exception { get; init; }
    public required DateTime Date { get; init; }
    public required string Url { get; init; }
    public required ImmutableArray<string> InvolvedModules { get; init; }
    public CrashReportStatus Status { get; init; } = CrashReportStatus.New;
    public string Comment { get; init; } = string.Empty;
}