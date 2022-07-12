using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
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

        private readonly ILogger _logger;
        private readonly AppDbContext _dbContext;

        public UserController(ILogger<UserController> logger, AppDbContext dbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
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
            UserRoleEntity? ApplyChanges(UserRoleEntity? existing) => existing switch
            {
                null => new() { UserId = (int) body.UserId, Role = body.Role },
                var entity => entity with { Role = body.Role }
            };
            if (await _dbContext.AddUpdateRemoveAndSaveAsync<UserRoleEntity>(x => x.UserId == body.UserId, ApplyChanges, ct))
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Set successful!"));

            return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Failed to set!"));
        }

        [HttpDelete("RemoveRole")]
        [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RemoveRole([FromBody] RemoveRoleBody body, CancellationToken ct)
        {
            UserRoleEntity? ApplyChanges(UserRoleEntity? existing) => existing switch
            {
                _ => null
            };
            if (await _dbContext.AddUpdateRemoveAndSaveAsync<UserRoleEntity>(x => x.UserId == body.UserId, ApplyChanges, ct))
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Deleted successful!"));

            return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Failed to delete!"));
        }
    }
}