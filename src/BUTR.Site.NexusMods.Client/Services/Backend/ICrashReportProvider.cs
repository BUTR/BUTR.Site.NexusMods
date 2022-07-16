using BUTR.Site.NexusMods.ServerClient;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services
{
    public interface ICrashReportProvider
    {
        Task<CrashReportModelPagingResponse?> GetCrashReports(int page, int pageSize, ICollection<Filtering> filterings, ICollection<Sorting> sortings, CancellationToken ct = default);

        Task<ICollection<string>> AutocompleteModId(string modId, CancellationToken ct = default);
        
        Task<bool> UpdateCrashReport(CrashReportModel crashReport, CancellationToken ct = default);
    }
}