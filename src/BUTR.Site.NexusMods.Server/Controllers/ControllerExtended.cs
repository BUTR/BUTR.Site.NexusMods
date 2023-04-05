using BUTR.Site.NexusMods.Server.Models.API;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace BUTR.Site.NexusMods.Server.Controllers;

public class ControllerExtended : ControllerBase
{
    [NonAction]
    protected ActionResult<APIResponse<T?>> Result<T>([ActionResultObjectValue] APIResponse<T?> value) => new OkObjectResult(value);

    [NonAction]
    protected ActionResult<APIResponse> Result() => new OkResult();
}