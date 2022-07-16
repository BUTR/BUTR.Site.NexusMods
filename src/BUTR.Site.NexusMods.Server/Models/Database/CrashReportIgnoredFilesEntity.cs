namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public class CrashReportIgnoredFilesEntity : IEntity
    {
        public string Filename { get; set; } = default!;
    }
}