using BUTR.Site.NexusMods.Client.Models;
using BUTR.Site.NexusMods.ServerClient;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services
{
    public sealed class DefaultCrashReportProvider : ICrashReportProvider
    {
        private readonly ICrashReportsClient _crashReportsClient;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenContainer _tokenContainer;

        public DefaultCrashReportProvider(ICrashReportsClient crashReportsClient, IHttpClientFactory httpClientFactory, ITokenContainer tokenContainer)
        {
            _crashReportsClient = crashReportsClient ?? throw new ArgumentNullException(nameof(crashReportsClient));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _tokenContainer = tokenContainer ?? throw new ArgumentNullException(nameof(tokenContainer));
        }

        public async Task<CrashReportModelPagingResponse?> GetCrashReports(int page, ICollection<Filtering> filterings, ICollection<Sorting> sortings, CancellationToken ct = default)
        {
            var token = await _tokenContainer.GetTokenAsync(ct);
            if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            {
                var crashReports = await DemoUser.GetCrashReports(_httpClientFactory).ToListAsync(ct);
                return new CrashReportModelPagingResponse(crashReports, new PagingMetadata(1, (int) Math.Ceiling((double) crashReports.Count / 10d), 10, crashReports.Count));
            }

            try
            {
                return await _crashReportsClient.PaginatedAsync(new CrashReportsPaginated(page, 10, filterings, sortings), ct);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> UpdateCrashReport(CrashReportModel crashReport, CancellationToken ct = default)
        {
            var token = await _tokenContainer.GetTokenAsync(ct);
            if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }

            try
            {
                await _crashReportsClient.UpdateAsync(crashReport, ct);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}