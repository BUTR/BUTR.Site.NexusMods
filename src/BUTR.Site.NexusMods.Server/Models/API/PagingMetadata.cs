using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Server.Models.API
{
    public sealed record PagingMetadata
    {
        [JsonIgnore]
        public bool HasPrevious => CurrentPage > 1;
        [JsonIgnore]
        public bool HasNext => CurrentPage < TotalPages;

        public int CurrentPage { get; init; }
        public int TotalPages { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
    }
}