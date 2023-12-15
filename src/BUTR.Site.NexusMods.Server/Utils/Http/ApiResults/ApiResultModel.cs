using Microsoft.AspNetCore.Mvc;

namespace BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

internal record ApiResultModel<TValue>(TValue? Value, ProblemDetails? Error)
{
    public static ApiResultModel<TValue> From(ApiResult<TValue> apiResult) => new(apiResult.Value, apiResult.Error);
}

internal record ApiResultModel(ProblemDetails? Error)
{
    public static ApiResultModel From(ApiResult apiResult) => new(apiResult.Error);
}