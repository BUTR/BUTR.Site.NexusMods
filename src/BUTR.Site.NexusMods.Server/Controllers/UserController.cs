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
        [ProducesResponseType(typeof(APIResponse<ProfileModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public ActionResult<APIResponse<ProfileModel?>> Profile() => Result(APIResponse.From(HttpContext.GetProfile(HttpContext.GetRole())));


        [HttpPost("SetRole")]
        [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse<string?>>> SetRole([FromQuery] SetRoleBody body, CancellationToken ct)
        {
            UserRoleEntity? ApplyChanges(UserRoleEntity? existing) => existing switch
            {
                null => new() { UserId = (int) body.UserId, Role = body.Role },
                _ => existing with { Role = body.Role }
            };
            if (await _dbContext.AddUpdateRemoveAndSaveAsync<UserRoleEntity>(x => x.UserId == body.UserId, ApplyChanges, ct))
                return Result(APIResponse.From("Set successful!"));

            return Result(APIResponse.Error<string>("Failed to set!"));
        }

        [HttpDelete("RemoveRole")]
        [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Moderator}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse<string?>>> RemoveRole([FromQuery] RemoveRoleBody body, CancellationToken ct)
        {
            UserRoleEntity? ApplyChanges(UserRoleEntity? existing) => existing switch
            {
                _ => null
            };
            if (await _dbContext.AddUpdateRemoveAndSaveAsync<UserRoleEntity>(x => x.UserId == body.UserId, ApplyChanges, ct))
                return Result(APIResponse.From("Deleted successful!"));

            return Result(APIResponse.Error<string>("Failed to delete!"));
        }
    }
}