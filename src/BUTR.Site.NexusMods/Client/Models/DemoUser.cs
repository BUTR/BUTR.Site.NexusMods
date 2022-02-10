using BUTR.Site.NexusMods.Shared.Helpers;
using BUTR.Site.NexusMods.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Models
{
    public static class DemoUser
    {
        private static readonly ProfileModel _profile = new(31179975, "Pickysaurus", "demo@demo.com", "https://forums.nexusmods.com/uploads/profile/photo-31179975.png", true, true, ApplicationRoles.User);
        private static readonly List<ModModel> _mods = new()
        {
            new("Demo Mod 1", "demo", 1),
            new("Demo Mod 2", "demo", 2),
            new("Demo Mod 3", "demo", 3),
            new("Demo Mod 4", "demo", 4),
        };
        private static List<CrashReportModel>? _crashReports;

        public static Task<ProfileModel> GetProfile() => Task.FromResult(_profile);
        public static Task<List<ModModel>> GetMods() => Task.FromResult(_mods);
        public static async Task<List<CrashReportModel>> GetCrashReports(IHttpClientFactory factory)
        {
            if (_crashReports is null)
            {
                var crm = new List<CrashReportModel>();
                try
                {
                    const string baseUrl = "https://crash.butr.dev/report/";
                    var client = factory.CreateClient("InternalReports");
                    var reports = new[] { "FC58E239", "7AA28856", "4EFF0B0A", "3DF57593" };
                    var crs = await Task.WhenAll(reports.Select(r => CrashReportParser.ParseUrl(client, r)));
                    crm = crs.Select(cr => new CrashReportModel(cr.Id, cr.Exception, DateTime.UtcNow, $"{baseUrl}{cr.Id2}.html")).ToList();
                }
                catch (Exception) { }
                _crashReports = crm;
            }
            return _crashReports;
        }
    }
}