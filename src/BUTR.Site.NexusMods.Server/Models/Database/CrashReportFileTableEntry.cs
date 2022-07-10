using System;

namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record CrashReportFileTableEntry
    {
        public string Filename { get; set; } = default!;

        public Guid CrashReportId { get; set; } = default!;
    }
}