using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace BUTR.Site.NexusMods.Server.Utils.APIResponses;

public sealed record APIResponseActionResult<TValue>(TValue? Value, ProblemDetails? Error) : IConvertToActionResult
{
    public static implicit operator APIResponseActionResult<TValue?>(TValue? value) => FromResult(value);
    public static implicit operator APIResponseActionResult<TValue?>(ProblemDetails error) => FromError(error);
    public static implicit operator APIResponseActionResult<TValue?>(APIResponseActionResultModel<TValue?> result) => new(result.Value, result.Error);

    public static APIResponseActionResult<TValue?> FromResult(TValue? data) => new(data, null);
    public static APIResponseActionResult<TValue?> FromError(ProblemDetails error) => new(default, error);

    public IActionResult Convert() => new APIResponseActionResultModel<TValue>(Value, Error);
}