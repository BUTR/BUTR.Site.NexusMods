namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record CrashReportFileEntity : IEntity
    {
        public string Filename { get; set; } = default!;

        public CrashReportEntity CrashReport { get; set; } = default!;
    }
}