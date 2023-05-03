using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
public sealed class UserController : ControllerExtended
{
    public sealed record SetRoleBody(uint UserId, string Role);
    public sealed record RemoveRoleBody(uint UserId);

    private readonly ILogger _logger;
    private readonly AppDbContext _dbContext;

    public UserController(ILogger<UserController> logger, AppDbContext dbContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }


    [HttpGet("Profile")]
    [Produces("application/json")]
    public ActionResult<APIResponse<ProfileModel?>> Profile() => APIResponse(HttpContext.GetProfile(HttpContext.GetRole()));


    [HttpPost("SetRole")]
    [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> SetRole([FromQuery] SetRoleBody body, CancellationToken ct)
    {
        NexusModsUserRoleEntity? ApplyChanges(NexusModsUserRoleEntity? existing) => existing switch
        {
            null => new() { NexusModsUserId = (int) body.UserId, Role = body.Role },
            _ => existing with { Role = body.Role }
        };
        if (await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsUserRoleEntity>(x => x.NexusModsUserId == body.UserId, ApplyChanges, ct))
            return APIResponse("Set successful!");

        return APIResponseError<string>("Failed to set!");
    }

    [HttpDelete("RemoveRole")]
    [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
    [Produces("application/json")]
    public async Task<ActionResult<APIResponse<string?>>> RemoveRole([FromQuery] RemoveRoleBody body, CancellationToken ct)
    {
        NexusModsUserRoleEntity? ApplyChanges(NexusModsUserRoleEntity? existing) => existing switch
        {
            _ => null
        };
        if (await _dbContext.AddUpdateRemoveAndSaveAsync<NexusModsUserRoleEntity>(x => x.NexusModsUserId == body.UserId, ApplyChanges, ct))
            return APIResponse("Deleted successful!");

        return APIResponseError<string>("Failed to delete!");
    }
}