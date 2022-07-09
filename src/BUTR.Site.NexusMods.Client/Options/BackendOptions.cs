namespace BUTR.Site.NexusMods.Client.Options
{
    public sealed record BackendOptions
    {
        public string Endpoint { get; init; } = default!;
    }
}