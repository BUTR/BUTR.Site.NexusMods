using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Server.Models.API;

public sealed record PagingMetadata
{
    public static PagingMetadata Empty(uint pageSize) => new() { CurrentPage = 1, TotalPages = 1, PageSize = pageSize, TotalCount = 0 };

    [JsonIgnore]
    public bool HasPrevious => CurrentPage > 1;
    [JsonIgnore]
    public bool HasNext => CurrentPage < TotalPages;

    public required uint CurrentPage { get; init; }
    public required uint TotalPages { get; init; }
    public required uint PageSize { get; init; }
    public required uint TotalCount { get; init; }
}