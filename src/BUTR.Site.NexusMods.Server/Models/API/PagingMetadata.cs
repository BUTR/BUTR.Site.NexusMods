using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Server.Models.API
{
    public sealed record PagingMetadata
    {
        public static PagingMetadata Empty(uint pageSize) => new() { CurrentPage = 1, TotalPages = 1, PageSize = pageSize, TotalCount = 0 };

        [JsonIgnore]
        public bool HasPrevious => CurrentPage > 1;
        [JsonIgnore]
        public bool HasNext => CurrentPage < TotalPages;

        public uint CurrentPage { get; init; }
        public uint TotalPages { get; init; }
        public uint PageSize { get; init; }
        public uint TotalCount { get; init; }
    }
}