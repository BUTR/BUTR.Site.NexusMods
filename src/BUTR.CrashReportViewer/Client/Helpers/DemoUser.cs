using BUTR.CrashReportViewer.Shared.Models;

using System;
using System.Collections.Generic;

namespace BUTR.CrashReportViewer.Client.Helpers
{
    public class DemoUser
    {
        private readonly struct DemoUserState
        {
            private const string StacktraceExample1 =
                @"Type: System.NullReferenceException
Message: Object reference not set to an instance of an object.
Source: SettlementCultureChanger
at SettlementCultureChanger.ChangeSettlementCulture.RevertCulture(Settlement settlement) in C:\Users\Enes\source\repos\SettlementCultureChanger\SettlementCultureChanger\ChangeSettlementCulture.cs:line 102";
            private const string StacktraceExample2 =
                @"Type: System.ArgumentNullException
Message: Value cannot be null. Parameter name: key
Source: mscorlib
at System.ThrowHelper.ThrowArgumentNullException(ExceptionArgument argument)";
            private const string StacktraceExample3 =
                @"Type: System.Reflection.TargetParameterCountException
Message: Parameter count mismatch.
Source: mscorlib
at System.Reflection.RuntimeMethodInfo.InvokeArgumentsCheck(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)";
            private const string StacktraceExample4 =
                @"Type: System.TypeInitializationException
Message: The type initializer for 'Bannerlord.BUTR.Shared.Helpers.TextObjectUtils' threw an exception.
Source: Bannerlord.Harmony
at Bannerlord.BUTR.Shared.Helpers.TextObjectUtils.Create(String value)";

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
                    new CrashReportModel(Guid.NewGuid(), StacktraceExample1, DateTime.UtcNow, "https://crash.butr.dev/report/FC58E239.html"),
                    new CrashReportModel(Guid.NewGuid(), StacktraceExample2, DateTime.UtcNow, "https://crash.butr.dev/report/7AA28856.html"),
                    new CrashReportModel(Guid.NewGuid(), StacktraceExample3, DateTime.UtcNow, "https://crash.butr.dev/report/4EFF0B0A.html"),
                    new CrashReportModel(Guid.NewGuid(), StacktraceExample4, DateTime.UtcNow, "https://crash.butr.dev/report/3DF57593.html"),
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