using BUTR.CrashReportViewer.Shared.Models;

using System;
using System.Collections.Generic;

namespace BUTR.CrashReportViewer.Client.Helpers
{
    public class DemoUser
    {
        private readonly struct DemoUserState
        {
            public static DemoUserState CreateNew() => new(
                new(31179975, "Pickysaurus", "demo@demo.com", "https://forums.nexusmods.com/uploads/profile/photo-31179975.png", true, true),
                new()
                {
                    new("Demo Mod 1", "demo", 1),
                    new("Demo Mod 2", "demo", 2),
                    new("Demo Mod 3", "demo", 3),
                    new("Demo Mod 4", "demo", 4),
                },
                new()
                {
                    new CrashReportModel(Guid.NewGuid(), DateTime.UtcNow, CrashReportStatus.New),
                    new CrashReportModel(Guid.NewGuid(), DateTime.UtcNow, CrashReportStatus.New),
                    new CrashReportModel(Guid.NewGuid(), DateTime.UtcNow, CrashReportStatus.New),
                    new CrashReportModel(Guid.NewGuid(), DateTime.UtcNow, CrashReportStatus.New),
                });


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


        public ProfileModel Profile => _state.Profile;
        public List<ModModel> Mods => _state.Mods;
        public List<CrashReportModel> CrashReports => _state.CrashReports;

        private DemoUserState _state = DemoUserState.CreateNew();

        public void Reset() => _state = DemoUserState.CreateNew();
    }
}