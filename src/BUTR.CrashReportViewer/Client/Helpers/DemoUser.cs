using BUTR.CrashReportViewer.Shared.Helpers;
using BUTR.CrashReportViewer.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BUTR.CrashReportViewer.Client.Helpers
{
    public class DemoUser
    {
        private readonly struct DemoUserState
        {
            public static async Task<DemoUserState> CreateAsync()
            {
                const string baseUrl = "https://crash.butr.dev/report/";
                return new(
                    new(31179975, "Pickysaurus", "demo@demo.com", "https://forums.nexusmods.com/uploads/profile/photo-31179975.png", true, true),
                    new()
                    {
                        new("Demo Mod 1", "demo", 1),
                        new("Demo Mod 2", "demo", 2),
                        new("Demo Mod 3", "demo", 3),
                        new("Demo Mod 4", "demo", 4),
                    },
                    new[]
                    {
                        CrashReportParser.Parse("FC58E239", DemoData.ReportFC58E239),
                        CrashReportParser.Parse("7AA28856", DemoData.Report7AA28856),
                        CrashReportParser.Parse("4EFF0B0A", DemoData.Report4EFF0B0A),
                        CrashReportParser.Parse("3DF57593", DemoData.Report3DF57593),
                    }.Select(cr => new CrashReportModel(cr.Id, cr.Exception, DateTime.UtcNow, $"{baseUrl}{cr.Id2}.html")).ToList()
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

        public static async Task<DemoUser> CreateAsync() => new(await DemoUserState.CreateAsync());


        public ProfileModel Profile => _state.Profile;
        public List<ModModel> Mods => _state.Mods;
        public List<CrashReportModel> CrashReports => _state.CrashReports;

        private readonly DemoUserState _state;

        private DemoUser(DemoUserState state) => _state = state;
    }
}