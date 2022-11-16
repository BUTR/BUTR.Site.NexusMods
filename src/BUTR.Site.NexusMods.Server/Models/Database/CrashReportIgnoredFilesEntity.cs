using System;

namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public class CrashReportIgnoredFilesEntity : IEntity
    {
        public required string Filename { get; init; }
        public required Guid Id { get; init; }
    }
}