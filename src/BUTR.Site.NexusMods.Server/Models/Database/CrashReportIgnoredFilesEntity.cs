using System;

namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed class CrashReportIgnoredFilesEntity : IEntity
    {
        public required string Filename { get; init; }
        public required Guid Id { get; init; }
    }
}