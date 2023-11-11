namespace BUTR.Site.NexusMods.Client.Utils;

public sealed record UserSettings
{
    public static readonly int[] AvailableCrashReportPageSizes = { 5, 10, 25, 50 };
    public static readonly int[] AvailablePageSizes = { 5, 10, 20, 50, 100 };
    public static readonly int DefaultCrashReportPageSize = 25;
    public static readonly int DefaultPageSize = 20;

    public int CrashReportPageSize { get; set; } = DefaultCrashReportPageSize;
    public int PageSize { get; set; } = DefaultPageSize;

    public string[] BlacklistedExceptions { get; set; }
}