using Microsoft.Extensions.Hosting;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services
{
    public static class DiscordConstants
    {
        public const string BUTRModAuthor = "butrmodauthor";
        public const string BUTRModerator = "butrmoderator";
        public const string BUTRAdministrator = "butradministrator";
    }
    
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
                new(DiscordConstants.BUTRModAuthor, "BUTR Mod Author", "Linked with the BUTR Site", 7), 
                new(DiscordConstants.BUTRModerator, "BUTR Moderator", "Moderator of BUTR Site", 7),
                new(DiscordConstants.BUTRAdministrator, "BUTR Administrator", "Administrator of BUTR Site", 7),
            });
        }
    }
}