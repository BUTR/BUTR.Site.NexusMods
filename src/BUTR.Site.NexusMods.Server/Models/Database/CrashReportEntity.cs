using System;
using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record CrashReportEntity : IEntity
    {
        public Guid Id { get; set; } = default!;

        public string GameVersion { get; set; } = default!;

        public string Exception { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = default!;

        public string[] ModIds { get; set; } = default!;

        public string[] InvolvedModIds { get; set; } = default!;

        public int[] ModNexusModsIds { get; set; } = default!;

        public string Url { get; set; } = default!;

        public ICollection<UserCrashReportEntity> UserCrashReports { get; set; } = default!;
    }
}