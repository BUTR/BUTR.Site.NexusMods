using BUTR.Site.NexusMods.Server.Models.API;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace BUTR.Site.NexusMods.Server.Controllers;

public class ControllerExtended : ControllerBase
{
    [NonAction]
    protected ActionResult<APIResponse<T?>> APIResponse<T>([ActionResultObjectValue] T? value) => BUTR.Site.NexusMods.Server.Models.API.APIResponse.From(value);
    [NonAction]
    protected ActionResult<APIResponse<T?>> APIResponseError<T>(string error) => BUTR.Site.NexusMods.Server.Models.API.APIResponse.Error<T>(error);
}