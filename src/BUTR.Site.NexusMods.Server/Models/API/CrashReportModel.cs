using System;
using System.Collections.Immutable;

namespace BUTR.Site.NexusMods.Server.Models.API
{
    public sealed record CrashReportModel
    {
        public Guid Id { get; init; }
        public int Version { get; init; }
        public string GameVersion { get; init; }
        public string Exception { get; init; }
        public DateTime Date { get; init; }
        public string Url { get; init; }
        public ImmutableArray<string> InvolvedModules { get; init; }
        public CrashReportStatus Status { get; init; } = CrashReportStatus.New;
        public string Comment { get; init; } = string.Empty;
    }
}