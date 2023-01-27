using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services
{
    public sealed class DiscordLinkedRolesService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly DiscordClient _client;

        public DiscordLinkedRolesService(ILogger<DiscordLinkedRolesService> logger, DiscordClient client)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var result = await _client.SetGlobalMetadata(new DiscordGlobalMetadata[]
            {
                new(DiscordConstants.BUTRModAuthor, "BUTR Mod Author", "Linked with the BUTR Site", 7),
                new(DiscordConstants.BUTRModerator, "BUTR Moderator", "Moderator of BUTR Site", 7),
                new(DiscordConstants.BUTRAdministrator, "BUTR Administrator", "Administrator of BUTR Site", 7),
                new(DiscordConstants.BUTRLinkedMods, "Linked Mods", "Minimal amount of linked mods required", 2),
            });
            if (!result)
                _logger.LogError("Failed to update Discord's global role-connection metadata!");
        }
    }
}