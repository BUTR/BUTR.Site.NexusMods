using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public sealed class DiscordLinkedRolesService : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public DiscordLinkedRolesService(ILogger<DiscordLinkedRolesService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var client = scope.ServiceProvider.GetRequiredService<DiscordClient>();

        var result = await client.SetGlobalMetadata(new DiscordGlobalMetadata[]
        {
            new(DiscordConstants.BUTRModAuthor, "BUTR Mod Author", "Linked with the BUTR Site", 7),
            new(DiscordConstants.BUTRModerator, "BUTR Moderator", "Moderator of BUTR Site", 7),
            new(DiscordConstants.BUTRAdministrator, "BUTR Administrator", "Administrator of BUTR Site", 7),
            new(DiscordConstants.BUTRLinkedMods, "Linked Mods", "Minimal amount of linked mods required", 2),
        }, CancellationToken.None);
        if (!result)
            _logger.LogError("Failed to update Discord's global role-connection metadata!");
    }
}