namespace BUTR.CrashReportViewer.Server.Options
{
    public record JwtOptions
    {
        public string SignKey { get; set; } = default!;
        public string EncryptionKey { get; set; } = default!;
    }
}