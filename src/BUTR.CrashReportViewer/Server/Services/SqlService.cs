using BUTR.CrashReportViewer.Server.Helpers;

using Microsoft.Extensions.Hosting;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.CrashReportViewer.Server.Services
{
    public class SqlService : IHostedService
    {
        private readonly SqlHelperInit _sqlHelperInit;

        public SqlService(SqlHelperInit sqlHelperInit)
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