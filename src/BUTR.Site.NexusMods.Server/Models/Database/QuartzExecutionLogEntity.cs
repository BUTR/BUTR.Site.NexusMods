using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record QuartzExecutionLogEntity : IEntity
{
    public required int LogId { get; init; }
    public required string RunInstanceId { get; init; }
    public required string JobName { get; init; }
    public required string JobGroup { get; init; }
    public required string TriggerName { get; init; }
    public required string TriggerGroup { get; init; }
    public required DateTimeOffset FireTimeUtc { get; init; }
    public required TimeSpan JobRunTime { get; init; }

    public required DateTimeOffset? ScheduleFireTimeUtc { get; init; }

    public required int RetryCount { get; init; }

    public required bool? IsSuccess { get; init; }
    public required bool? IsException { get; init; }
    public required bool? IsVetoed { get; init; }

    public required string? ErrorMessage { get; init; }
    public QuartzExecutionLogDetailEntity? ExecutionLogDetail { get; init; }
    public required string? Result { get; init; }
    public required string? ReturnCode { get; init; }

    public DateTimeOffset DateAddedUtc { get; init; } = DateTimeOffset.UtcNow;
    public string? MachineName { get; init; } = Environment.MachineName;

    public DateTimeOffset? GetFinishTimeUtc() => FireTimeUtc.Add(JobRunTime);
}