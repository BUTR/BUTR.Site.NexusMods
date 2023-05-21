namespace BUTR.Site.NexusMods.Server.Models.API
{
    public sealed record PagingAdditionalMetadata
    {
        public static PagingAdditionalMetadata Empty(uint pageSize) => new() { QueryExecutionTimeMilliseconds = 0 };

        public required uint QueryExecutionTimeMilliseconds { get; init; }
    }
}