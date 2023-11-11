using Microsoft.AspNetCore.Mvc;

namespace BUTR.Site.NexusMods.Server.Models.API;

public abstract record APIResponse
{
    public static APIResponse<T?> FromResult<T>(T? data, ProblemDetails? error = null) => new(data, error);
    public static APIResponse<T?> FromError<T>(ProblemDetails error) => new(default, error);
}
public record APIResponse<T>(T? Value, ProblemDetails? Error) : APIResponse;

public record APIStreamingResponse(ProblemDetails? Error) : APIResponse;