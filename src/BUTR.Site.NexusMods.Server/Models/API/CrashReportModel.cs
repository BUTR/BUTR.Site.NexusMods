using System;
using System.Collections.Immutable;

namespace BUTR.Site.NexusMods.Server.Models.API
{
    public sealed record CrashReportModel(Guid Id, string GameVersion, string Exception, DateTime Date, string Url, ImmutableArray<string> InvolvedModules)
    {
        public CrashReportStatus Status { get; init; } = default!;
        public string Comment { get; init; } = default!;
    }
}