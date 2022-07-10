using System;
using System.Collections.Immutable;

namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record CrashReportTableEntry
    {
        public Guid Id { get; set; } = default!;

        public string GameVersion { get; set; } = default!;

        public string Exception { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = default!;

        public ImmutableArray<string> ModIds { get; set; } = default!;

        public ImmutableArray<string> InvolvedModIds { get; set; } = default!;

        public ImmutableArray<int> ModNexusModsIds { get; set; } = default!;

        public string Url { get; set; } = default!;

        public ImmutableArray<UserCrashReportTableEntry> UserCrashReports { get; set; } = default!;
    }
}