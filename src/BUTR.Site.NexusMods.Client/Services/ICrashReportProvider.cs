using BUTR.Site.NexusMods.Shared.Models;
using BUTR.Site.NexusMods.Shared.Models.API;

using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services
{
    public interface ICrashReportProvider
    {
        Task<PagingResponse<CrashReportModel>?> GetCrashReports(int page, CancellationToken ct = default);
        Task<bool> UpdateCrashReport(CrashReportModel crashReport, CancellationToken ct = default);
    }
}