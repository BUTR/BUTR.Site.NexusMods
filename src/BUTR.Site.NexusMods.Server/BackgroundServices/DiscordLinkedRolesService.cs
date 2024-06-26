using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.BackgroundServices;

[HostedService]
public sealed class DiscordLinkedRolesService : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public DiscordLinkedRolesService(ILogger<DiscordLinkedRolesService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var client = scope.ServiceProvider.GetRequiredService<IDiscordClient>();

        var result = await client.SetGlobalMetadataAsync(new DiscordGlobalMetadata[]
        {
            new(DiscordConstants.BUTRModAuthor, "BUTR Mod Author", "Linked with the BUTR Site", DiscordGlobalMetadataType.BOOLEAN_EQUAL),
            new(DiscordConstants.BUTRModerator, "BUTR Moderator", "Moderator of BUTR Site", DiscordGlobalMetadataType.BOOLEAN_EQUAL),
            new(DiscordConstants.BUTRAdministrator, "BUTR Administrator", "Administrator of BUTR Site", DiscordGlobalMetadataType.BOOLEAN_EQUAL),
            new(DiscordConstants.BUTRLinkedMods, "Linked Mods", "Minimal amount of linked mods required", DiscordGlobalMetadataType.INTEGER_GREATER_THAN_OR_EQUAL),
        }, CancellationToken.None);
        if (!result)
            _logger.LogError("Failed to update Discord's global role-connection metadata!");
    }
}