namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record QuartzExecutionLogDetailEntity
{
    public required string? ExecutionDetails { get; init; }

    public required int? ErrorCode { get; init; }
    public required string? ErrorStackTrace { get; init; }
    public required string? ErrorHelpLink { get; init; }
}