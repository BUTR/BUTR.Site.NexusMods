using Microsoft.Extensions.Hosting;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services.Database
{
    public class SeederService : IHostedService
    {
        private readonly SeederProvider _sqlHelperInit;

        public SeederService(SeederProvider sqlHelperInit)
        {
            _sqlHelperInit = sqlHelperInit ?? throw new ArgumentNullException(nameof(sqlHelperInit));
        }

        public async Task StartAsync(CancellationToken ct)
        {
            await _sqlHelperInit.CreateTablesIfNotExistAsync(ct);
        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}