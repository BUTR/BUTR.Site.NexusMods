namespace BUTR.Site.NexusMods.Server.Options
{
    public record ServiceUrlsOptions
    {
        public string NexusMods { get; init; }
        public string CrashReporter { get; init; }
    }
}