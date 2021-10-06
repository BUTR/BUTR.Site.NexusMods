using BUTR.CrashReportViewer.Shared.Helpers;
using BUTR.CrashReportViewer.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BUTR.CrashReportViewer.Client.Helpers
{
    public class DemoUser
    {
        private readonly struct DemoUserState
        {
            public static async Task<DemoUserState> Create(IHttpClientFactory factory)
            {
                var crm = new List<CrashReportModel>();
                try
                {
                    var client = factory.CreateClient("CrashReporterDemo");
                    var reports = new[] { "FC58E239", "7AA28856", "4EFF0B0A", "3DF57593" };
                    var crs = await Task.WhenAll(reports.Select(r => CrashReportParser.ParseUrl(client, r)));
                    crm = crs.Select(cr => new CrashReportModel(cr.Id, cr.Exception, DateTime.UtcNow, $"{client.BaseAddress}{cr.Id2}.html")).ToList();
                }
                catch (Exception) { }

                return new(
                    new(31179975, "Pickysaurus", "demo@demo.com", "https://forums.nexusmods.com/uploads/profile/photo-31179975.png", true, true),
                    new()
                    {
                        new("Demo Mod 1", "demo", 1),
                        new("Demo Mod 2", "demo", 2),
                        new("Demo Mod 3", "demo", 3),
                        new("Demo Mod 4", "demo", 4),
                    },
                    crm
                );
            }


            public readonly ProfileModel Profile;
            public readonly List<ModModel> Mods;
            public readonly List<CrashReportModel> CrashReports;

            private DemoUserState(ProfileModel profile, List<ModModel> mods, List<CrashReportModel> crashReports)
            {
                Profile = profile;
                Mods = mods;
                CrashReports = crashReports;
            }
        }

        public static async Task<DemoUser> Create(IHttpClientFactory factory) => new(await DemoUserState.Create(factory));


        public ProfileModel Profile => _state.Profile;
        public List<ModModel> Mods => _state.Mods;
        public List<CrashReportModel> CrashReports => _state.CrashReports;

        private readonly DemoUserState _state;

        private DemoUser(DemoUserState state) => _state = state;
    }
}