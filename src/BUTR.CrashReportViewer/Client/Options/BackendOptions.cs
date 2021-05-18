namespace BUTR.CrashReportViewer.Client.Options
{
    public record BackendOptions
    {
        public string Endpoint { get; init; } = default!;
    }
}