using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record CrashReportEntity : IEntity
    {
        public required Guid Id { get; init; }

        public required int Version { get; init; }

        public required string GameVersion { get; init; }

        public required string Exception { get; init; }

        public required DateTime CreatedAt { get; init; }

        public required string[] ModIds { get; init; }

        public required Dictionary<string, string> ModIdToVersion { get; init; }

        public required string[] InvolvedModIds { get; init; }

        public required int[] ModNexusModsIds { get; init; }

        public required string Url { get; init; }

        public CrashReportEntity() { }

        [SetsRequiredMembers]
        public CrashReportEntity(Guid id) => Id = id;
    }
}