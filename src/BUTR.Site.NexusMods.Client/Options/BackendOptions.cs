namespace BUTR.Site.NexusMods.Client.Options;

public sealed record BackendOptions
{
    public required string Endpoint { get; init; }
}