using System;

namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record QuartzExecutionLogEntity : IEntity
    {
        public long LogId { get; init; }
        public string? RunInstanceId { get; init; }
        public required QuartzLogType LogType { get; init; }
        public string? JobName { get; set; }
        public string? JobGroup { get; set; }
        public string? TriggerName { get; init; }
        public string? TriggerGroup { get; init; }
        public DateTimeOffset? ScheduleFireTimeUtc { get; init; }
        public DateTimeOffset? FireTimeUtc { get; init; }
        public TimeSpan? JobRunTime { get; set; }
        public int? RetryCount { get; init; }
        public string? Result { get; set; }
        public string? ErrorMessage { get; set; }
        public bool? IsVetoed { get; set; }
        public bool? IsException { get; set; }
        public bool? IsSuccess { get; set; }
        public string? ReturnCode { get; set; }
        public DateTimeOffset DateAddedUtc { get; init; } = DateTimeOffset.UtcNow;
        public QuartzExecutionLogDetailEntity? ExecutionLogDetail { get; set; }
        public string? MachineName { get; set; } = Environment.MachineName;

        public DateTimeOffset? GetFinishTimeUtc() => FireTimeUtc?.Add(JobRunTime ?? TimeSpan.Zero);
    }
}