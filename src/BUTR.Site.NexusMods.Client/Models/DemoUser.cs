using BUTR.CrashReport.Bannerlord.Parser;
using BUTR.Site.NexusMods.ServerClient;
using BUTR.Site.NexusMods.Shared.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using ExceptionModel = BUTR.CrashReport.Models.ExceptionModel;

namespace BUTR.Site.NexusMods.Client.Models;

public static class DemoUser
{
    private static readonly ProfileModel _profile = new(
            nexusModsUserId: 31179975,
            name: "Pickysaurus",
            email: "demo@demo.com",
            profileUrl: "https://forums.nexusmods.com/uploads/profile/photo-31179975.png",
            isSupporter: true,
            isPremium: true,
            role: ApplicationRoles.User,
            steamUserId: null!,
            gogUserId: null!,
            discordUserId: null!,
            gitHubUserId: null!,
            hasTenantGame: true,
            availableTenants: new List<ProfileTenantModel> { new(tenantId: 1, name: "Bannerlord") });
    private static readonly List<UserLinkedModModel> _mods = new()
    {
        new(nexusModsModId: 1, name: "Demo Mod 1", ownerNexusModsUserIds: Array.Empty<int>(), allowedNexusModsUserIds: Array.Empty<int>(), manuallyLinkedNexusModsUserIds: Array.Empty<int>(), knownModuleIds: Array.Empty<string>(), manuallyLinkedModuleIds: Array.Empty<string>()),
        new(nexusModsModId: 2, name: "Demo Mod 2", ownerNexusModsUserIds: Array.Empty<int>(), allowedNexusModsUserIds: Array.Empty<int>(), manuallyLinkedNexusModsUserIds: Array.Empty<int>(), knownModuleIds: Array.Empty<string>(), manuallyLinkedModuleIds: Array.Empty<string>()),
        new(nexusModsModId: 3, name: "Demo Mod 3", ownerNexusModsUserIds: Array.Empty<int>(), allowedNexusModsUserIds: Array.Empty<int>(), manuallyLinkedNexusModsUserIds: Array.Empty<int>(), knownModuleIds: Array.Empty<string>(), manuallyLinkedModuleIds: Array.Empty<string>()),
        new(nexusModsModId: 4, name: "Demo Mod 4", ownerNexusModsUserIds: Array.Empty<int>(), allowedNexusModsUserIds: Array.Empty<int>(), manuallyLinkedNexusModsUserIds: Array.Empty<int>(), knownModuleIds: Array.Empty<string>(), manuallyLinkedModuleIds: Array.Empty<string>()),
    };
    private static List<CrashReportModel2>? _crashReports;

    public static Task<ProfileModel> GetProfile() => Task.FromResult(_profile);
    public static IAsyncEnumerable<UserLinkedModModel> GetMods() => _mods.ToAsyncEnumerable();
    public static async IAsyncEnumerable<CrashReportModel2> GetCrashReports(IHttpClientFactory factory)
    {
        static string GetException(ExceptionModel? exception, bool inner = false) => exception is null ? string.Empty : $"""

{(inner ? "Inner " : string.Empty)}Exception information
Type: {exception.Type}
Message: {exception.Message}
CallStack:
{exception.CallStack}

{GetException(exception.InnerException, true)}
""";

        static async Task<(string, CrashReport.Models.CrashReportModel)> DownloadReport(HttpClient client, string id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{id}.html");
            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            return (id, CrashReportParser.ParseLegacyHtml(10, content));
        }

        if (_crashReports is null)
        {
            var crm = new List<CrashReportModel2>();
            const string baseUrl = "https://report.butr.link/";
            var client = factory.CreateClient("InternalReports");
            var reports = new[] { "4DDA8D", "6FB0EF", "2AE0EA", "F966E3" };
            var contents = await Task.WhenAll(reports.Select(r => DownloadReport(client, r)));
            foreach (var (id, cr) in contents)
            {
                var report = new CrashReportModel2(
                    id: cr.Id,
                    version: cr.Version,
                    gameVersion: cr.Metadata.GameVersion,
                    exceptionType: cr.Exception.Type,
                    exception: GetException(cr.Exception),
                    date: DateTimeOffset.UtcNow,
                    url: $"{baseUrl}{id}.html",
                    involvedModules: cr.Modules.Select(x => x.Id).ToList(),
                    status: CrashReportStatus.New,
                    comment: string.Empty);
                crm.Add(report);
                yield return report;

            }
            _crashReports = crm;
        }
        else
        {
            foreach (var crashReport in _crashReports)
            {
                yield return crashReport;
            }
        }
    }
}