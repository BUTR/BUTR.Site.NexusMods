namespace BUTR.Site.NexusMods.Client.Options;

public sealed record CrashReporterOptions
{
    public required string Endpoint { get; init; }
}