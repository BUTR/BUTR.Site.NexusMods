using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services
{
    public class CrashReporterClient
    {
        private readonly HttpClient _httpClient;

        public CrashReporterClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string> GetCrashReportAsync(string id, CancellationToken ct)
        {
            return await _httpClient.GetStringAsync($"{id}.html", ct);
        }
    }
}