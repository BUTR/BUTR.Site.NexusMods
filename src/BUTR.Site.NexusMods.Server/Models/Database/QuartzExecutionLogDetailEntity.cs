namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record QuartzExecutionLogDetailEntity
{
    public string? ExecutionDetails { get; set; }
    public string? ErrorStackTrace { get; set; }
    public int? ErrorCode { get; set; }
    public string? ErrorHelpLink { get; set; }
}