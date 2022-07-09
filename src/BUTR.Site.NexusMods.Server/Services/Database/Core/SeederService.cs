using Microsoft.Extensions.Hosting;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services.Database
{
    public sealed class SeederService : IHostedService
    {
        private readonly SeederProvider _seeder;

        public SeederService(SeederProvider seeder)
        {
            _seeder = seeder ?? throw new ArgumentNullException(nameof(seeder));
        }

        public async Task StartAsync(CancellationToken ct)
        {
            await _seeder.CreateTablesIfNotExistAsync(ct);
        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}