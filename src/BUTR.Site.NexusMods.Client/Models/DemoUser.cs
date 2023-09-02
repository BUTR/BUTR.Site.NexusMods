﻿using BUTR.Site.NexusMods.ServerClient;
using BUTR.Site.NexusMods.Shared.Helpers;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Models;

public static class DemoUser
{
    private static readonly ProfileModel _profile =
        new(31179975, "Pickysaurus", "demo@demo.com", "https://forums.nexusmods.com/uploads/profile/photo-31179975.png", true, true, ApplicationRoles.User, null, null, null, true);
    private static readonly List<NexusModsModModel> _mods = new()
    {
        new(1, "Demo Mod 1", ImmutableArray<int>.Empty, ImmutableArray<int>.Empty, ImmutableArray<string>.Empty, ImmutableArray<string>.Empty),
        new(2, "Demo Mod 2", ImmutableArray<int>.Empty, ImmutableArray<int>.Empty, ImmutableArray<string>.Empty, ImmutableArray<string>.Empty),
        new(3, "Demo Mod 3", ImmutableArray<int>.Empty, ImmutableArray<int>.Empty, ImmutableArray<string>.Empty, ImmutableArray<string>.Empty),
        new(4, "Demo Mod 4", ImmutableArray<int>.Empty, ImmutableArray<int>.Empty, ImmutableArray<string>.Empty, ImmutableArray<string>.Empty),
    };
    private static List<CrashReportModel>? _crashReports;

    public static Task<ProfileModel> GetProfile() => Task.FromResult(_profile);
    public static IAsyncEnumerable<NexusModsModModel> GetMods() => _mods.ToAsyncEnumerable();
    public static async IAsyncEnumerable<CrashReportModel> GetCrashReports(IHttpClientFactory factory)
    {
        static async Task<CrashReport> DownloadReport(HttpClient client, string id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{id}.html");
            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            return CrashReportParser.Parse(id, content);
        }

        if (_crashReports is null)
        {
            var crm = new List<CrashReportModel>();
            const string baseUrl = "https://report.butr.link/";
            var client = factory.CreateClient("InternalReports");
            var reports = new[] { "4DDA8D", "6FB0EF", "2AE0EA", "F966E3" };
            var contents = await Task.WhenAll(reports.Select(r => DownloadReport(client, r)));
            foreach (var cr in contents)
            {
                var report = new CrashReportModel(cr.Id, cr.Version, cr.GameVersion, cr.ExceptionType, cr.Exception, DateTime.UtcNow, $"{baseUrl}{cr.Id2}.html", cr.Modules.Select(x => x.Id).ToArray(), CrashReportStatus.New, string.Empty);
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