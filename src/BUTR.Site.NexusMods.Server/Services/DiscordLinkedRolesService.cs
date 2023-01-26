using Microsoft.Extensions.Hosting;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public sealed class DiscordLinkedRolesService : BackgroundService
{
    private readonly DiscordClient _client;

    public DiscordLinkedRolesService(DiscordClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }
        
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _client.SetGlobalMetadata(new DiscordGlobalMetadata[]
        {
            new("butrmodauthor", "BUTR Mod Author", "Linked with the BUTR Site", 7), 
            new("butrmoderator", "BUTR Moderator", "Moderator of BUTR Site", 7),
            new("butradministrator", "BUTR Administrator", "Administrator of BUTR Site", 7),
        });
    }
}