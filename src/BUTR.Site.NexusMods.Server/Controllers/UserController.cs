using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Services.Database;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers
{
    [ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
    public sealed class UserController : ControllerBase
    {
        public sealed record SetRoleBody(uint UserId, string Role);
        public sealed record RemoveRoleBody(uint UserId);

        public sealed record AddAllowedModBody(uint UserId, string ModId);
        public sealed record RemoveAllowedModBody(uint UserId, string ModId);

        
        private readonly ILogger _logger;
        private readonly UserRoleProvider _userRoleProvider;
        private readonly UserAllowedModsProvider _userAllowedModsProvider;

        public UserController(ILogger<UserController> logger, UserRoleProvider userRoleProvider, UserAllowedModsProvider userAllowedModsProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userRoleProvider = userRoleProvider ?? throw new ArgumentNullException(nameof(userRoleProvider));
            _userAllowedModsProvider = userAllowedModsProvider ?? throw new ArgumentNullException(nameof(userAllowedModsProvider));
        }

        
        [HttpGet("Profile")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ProfileModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public ActionResult Profile() => StatusCode(StatusCodes.Status200OK, HttpContext.GetProfile(HttpContext.GetRole()));
        

        [HttpPost("SetRole")]
        [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> SetRole([FromBody] SetRoleBody body, CancellationToken ct)
        {
            if (await _userRoleProvider.UpsertAsync(new UserRoleTableEntry { UserId = (int) body.UserId, Role = body.Role }, ct) is not null)
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Upserted!"));

            return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Failed to upsert!"));
        }

        [HttpDelete("RemoveRole")]
        [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RemoveRole([FromBody] RemoveRoleBody body, CancellationToken ct)
        {
            if (await _userRoleProvider.FindAsync((int) body.UserId, ct) is not { } entry)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Not found!"));

            if (await _userRoleProvider.DeleteAsync(entry, ct))
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Deleted successful!"));

            return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Failed to delete!"));
        }


        [HttpPost("AddAllowedMod")]
        [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AddAllowedMod([FromBody] AddAllowedModBody body, CancellationToken ct)
        {
            if (await _userAllowedModsProvider.FindAsync((int) body.UserId, ct) is not { } entry)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Not found!"));
            
            if (await _userAllowedModsProvider.UpsertAsync(new UserAllowedModsTableEntry { UserId = (int) body.UserId, AllowedModIds = entry.AllowedModIds.Add(body.ModId) }, ct) is not null)
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Added successful!"));

            return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Failed to upsert!"));
        }

        [HttpDelete("RemoveAllowedMod")]
        [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RemoveAllowedMod([FromBody] RemoveAllowedModBody body, CancellationToken ct)
        {
            if (await _userAllowedModsProvider.FindAsync((int) body.UserId, ct) is not { } entry)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Not found!"));

            if (await _userAllowedModsProvider.UpsertAsync(new UserAllowedModsTableEntry { UserId = (int) body.UserId, AllowedModIds = entry.AllowedModIds.Remove(body.ModId) }, ct) is not null)
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Removed successful!"));

            return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Failed to delete!"));
        }
    }
}