using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

public sealed class ApiResult : IConvertToActionResult
{
    public static implicit operator ObjectResult(ApiResult apiResult) => new(ApiResultModel.From(apiResult));

    public static ApiResult FromResult(HttpContext httpContext, int? status = null)
    {
        return new ApiResult(null, status);
    }
    
    public static ApiResult FromError(HttpContext httpContext, ProblemDetails? error)
    {
        return new ApiResult(error, null);
    }
    
    public ProblemDetails? Error { get; }

    public int? Status { get; }
    
    private ApiResult(ProblemDetails? error, int? status)
    {
        Status = status;
        Error = error;
    }

    IActionResult IConvertToActionResult.Convert() => Convert();
    public ObjectResult Convert() => this;
}

public record ApiResult<TValue> : IConvertToActionResult
{
    public static implicit operator ObjectResult(ApiResult<TValue> apiResult) => new(ApiResultModel<TValue>.From(apiResult));
    
    public static implicit operator ApiResult<TValue?>(ApiResult apiResult) => new(default, null, apiResult.Error);

    public static ApiResult<TValue?> FromResult(HttpContext httpContext, TValue data, int? status = null)
    {
        return new ApiResult<TValue?>(data, status, null);
    }

    public TValue? Value { get; }
    public ProblemDetails? Error { get; }
    
    public int? Status { get; }
    
    protected ApiResult(TValue? value, int? status, ProblemDetails? error)
    {
        Value = value;
        Status = status;
        Error = error;
    }

    public virtual IActionResult Convert() => (ObjectResult) this;
}