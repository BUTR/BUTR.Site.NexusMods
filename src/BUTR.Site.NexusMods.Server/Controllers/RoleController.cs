using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Services.Database;
using BUTR.Site.NexusMods.Shared.Helpers;
using BUTR.Site.NexusMods.Shared.Models.API;

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
    public class RoleController : ControllerBase
    {
        public record RoleUpsertQuery(uint UserId, string Role);
        public record RoleDeleteQuery(uint UserId);

        private readonly ILogger _logger;
        private readonly RoleProvider _roleProvider;

        public RoleController(ILogger<ModListController> logger, RoleProvider roleProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _roleProvider = roleProvider ?? throw new ArgumentNullException(nameof(roleProvider));
        }

        [HttpPost("")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Upsert([FromBody] RoleUpsertQuery query, CancellationToken ct)
        {
            if (!User.IsInRole(ApplicationRoles.Administrator) && !User.IsInRole(ApplicationRoles.Moderator))
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Insufficient rights!"));

            if (await _roleProvider.UpsertAsync(new RoleEntry { UserId = query.UserId, Role = query.Role }, ct) is not null)
            {
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Upserted!"));
            }

            return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Failed to upsert!"));
        }

        [HttpDelete("")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StandardResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Delete([FromBody] RoleDeleteQuery query, CancellationToken ct)
        {
            if (!User.IsInRole(ApplicationRoles.Administrator) && !User.IsInRole(ApplicationRoles.Moderator))
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Insufficient rights!"));

            if (await _roleProvider.FindAsync(query.UserId, ct) is not { } role)
                return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Not found!"));

            if (await _roleProvider.DeleteAsync(role, ct))
                return StatusCode(StatusCodes.Status200OK, new StandardResponse("Deleted successful!"));

            return StatusCode(StatusCodes.Status400BadRequest, new StandardResponse("Failed to delete!"));
        }
    }
}