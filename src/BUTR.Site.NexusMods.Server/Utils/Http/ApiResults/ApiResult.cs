using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

public record ApiResult(ProblemDetails? Error) : IApiResult, IConvertToActionResult
{
    public static implicit operator ObjectResult(ApiResult apiResult) => new(apiResult);
    
    public static ApiResult FromError(ProblemDetails? error) => new(error);
    
    public IActionResult Convert() => (ObjectResult) this;
}

public sealed record ApiResult<TValue>(TValue? Value, ProblemDetails? Error) : IApiResult, IConvertToActionResult
{
    public static implicit operator ApiResult<TValue?>(TValue? value) => FromResult(value);
    public static implicit operator ApiResult<TValue?>(ProblemDetails? error) => FromError(error);
    public static implicit operator ApiResult<TValue?>(ApiResult apiResult) => FromError(apiResult.Error);

    public static implicit operator ObjectResult(ApiResult<TValue> apiResult) => new(apiResult);

    public static ApiResult<TValue?> FromResult(TValue? data, ProblemDetails? error = null) => new(data, error);
    public static ApiResult<TValue?> FromError(ProblemDetails? error) => new(default, error);
    
    public IActionResult Convert() => (ObjectResult) this;
}