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
                @"Exception information
Type: System.NullReferenceException
Message: Object reference not set to an instance of an object.
Source: SettlementCultureChanger
CallStack:
at SettlementCultureChanger.ChangeSettlementCulture.RevertCulture(Settlement settlement) in C:\Users\Enes\source\repos\SettlementCultureChanger\SettlementCultureChanger\ChangeSettlementCulture.cs:line 102
at SettlementCultureChanger.ChangeSettlementCulture.OnSettlementOwnerChanged(Settlement settlement, Boolean arg2, Hero arg3, Hero arg4, Hero arg5, ChangeOwnerOfSettlementDetail detail) in C:\Users\Enes\source\repos\SettlementCultureChanger\SettlementCultureChanger\ChangeSettlementCulture.cs:line 66
at TaleWorlds.CampaignSystem.MbEvent`6.InvokeList(EventHandlerRec`6 list, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
at TaleWorlds.CampaignSystem.CampaignEvents.OnSettlementOwnerChanged(Settlement settlement, Boolean openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementDetail detail)
at TaleWorlds.CampaignSystem.CampaignEventDispatcher.OnSettlementOwnerChanged(Settlement settlement, Boolean openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementDetail detail)
at TaleWorlds.CampaignSystem.Actions.ChangeOwnerOfSettlementAction.ApplyInternal(Settlement settlement, Hero newOwner, Hero capturerHero, ChangeOwnerOfSettlementDetail detail)
at TaleWorlds.CampaignSystem.KingdomManager.SiegeCompleted(Settlement settlement, MobileParty capturerParty, Boolean isWin, Boolean isSiege)
at TaleWorlds.CampaignSystem.MbEvent`4.InvokeList(EventHandlerRec`4 list, T1 t1, T2 t2, T3 t3, T4 t4)
at TaleWorlds.CampaignSystem.CampaignEvents.SiegeCompleted(Settlement siegeSettlement, MobileParty attackerParty, Boolean isWin, Boolean isSiege)
at TaleWorlds.CampaignSystem.CampaignEventDispatcher.SiegeCompleted(Settlement siegeSettlement, MobileParty attackerParty, Boolean isWin, Boolean isSiege)
at TaleWorlds.CampaignSystem.MapEvent.FinalizeEventAux()
at TaleWorlds.CampaignSystem.MapEvent.FinishBattle()
at TaleWorlds.CampaignSystem.MapEvent.Update()
at TaleWorlds.CampaignSystem.MapEventManager.Tick()
at TaleWorlds.CampaignSystem.Campaign.Tick(Single dt)
at TaleWorlds.CampaignSystem.Campaign.RealTick(Single realDt)
at TaleWorlds.CampaignSystem.MapState.OnMapModeTick(Single dt)
at TaleWorlds.Core.GameStateManager.OnTick(Single dt)
at TaleWorlds.Core.Game.OnTick(Single dt)
at TaleWorlds.Core.GameManagerBase.OnTick(Single dt)
at TaleWorlds.MountAndBlade.Module.OnApplicationTick_Patch2(Module this, Single dt)";
            private const string StacktraceExample2 =
                @"Exception information
Type: System.ArgumentNullException
Message: Value cannot be null. Parameter name: key
Source: mscorlib
CallStack:
at System.ThrowHelper.ThrowArgumentNullException(ExceptionArgument argument)
at System.Collections.Generic.Dictionary`2.FindEntry(TKey key)
at System.Collections.Generic.Dictionary`2.TryGetValue(TKey key, TValue& value)
at TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.CaravansCampaignBehavior.UpdateAverageValues()
at TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.CaravansCampaignBehavior.OnSessionLaunched(CampaignGameStarter campaignGameStarter)
at TaleWorlds.CampaignSystem.MbEvent`1.InvokeList(EventHandlerRec`1 list, T t)
at TaleWorlds.CampaignSystem.CampaignEvents.OnSessionStart(CampaignGameStarter campaignGameStarter)
at TaleWorlds.CampaignSystem.CampaignEventDispatcher.OnSessionStart(CampaignGameStarter campaignGameStarter)
at TaleWorlds.CampaignSystem.Campaign.OnSessionStart(CampaignGameStarter starter)
at TaleWorlds.CampaignSystem.Campaign.DoLoadingForGameType(GameTypeLoadingStates gameTypeLoadingState, GameTypeLoadingStates& nextState)
at StoryMode.CampaignStoryMode.DoLoadingForGameType(GameTypeLoadingStates gameTypeLoadingState, GameTypeLoadingStates& nextState)
at TaleWorlds.Core.GameType.DoLoadingForGameType()
at SandBox.CampaignGameManager.DoLoadingForGameManager(GameManagerLoadingSteps gameManagerLoadingStep, GameManagerLoadingSteps& nextStep)
at TaleWorlds.Core.GameManagerBase.DoLoadingForGameManager()
at TaleWorlds.MountAndBlade.GameLoadingState.OnTick(Single dt)
at TaleWorlds.Core.GameStateManager.OnTick(Single dt)
at TaleWorlds.MountAndBlade.Module.OnApplicationTick_Patch2(Module this, Single dt)";
            private const string StacktraceExample3 =
                @"Exception information
Type: System.Reflection.TargetParameterCountException
Message: Parameter count mismatch.
Source: mscorlib
CallStack:
at System.Reflection.RuntimeMethodInfo.InvokeArgumentsCheck(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)
at System.Reflection.RuntimeMethodInfo.Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)
at System.Reflection.MethodBase.Invoke(Object obj, Object[] parameters)
at BannerlordTweaks.Patches.DoSmeltingPatch.Postfix(CraftingCampaignBehavior __instance, EquipmentElement equipmentElement) in C:\Users\Shmoove\source\repos\Jirow13\BannerlordTweaks\Patches\CraftingCampaignBehaviourPatches.cs:line 25
at TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.CraftingCampaignBehavior.DoSmelting_Patch1(CraftingCampaignBehavior this, Hero hero, EquipmentElement equipmentElement)
at TaleWorlds.CampaignSystem.ViewModelCollection.Craft.Smelting.SmeltingVM.SmeltSelectedItems(Hero currentCraftingHero)
at TaleWorlds.CampaignSystem.ViewModelCollection.CraftingVM.ExecuteMainAction()
at SandBox.GauntletUI.AutoGenerated.Crafting__TaleWorlds_CampaignSystem_ViewModelCollection_CraftingVM.EventListenerOf_widget_8_4_0_0(Widget widget, String commandName, Object[] args)
at TaleWorlds.GauntletUI.Widget.EventFired(String eventName, Object[] args)
at TaleWorlds.GauntletUI.ButtonWidget.HandleClick()
at TaleWorlds.GauntletUI.ButtonWidget.OnMouseReleased()
at TaleWorlds.GauntletUI.EventManager.MouseUp()
at TaleWorlds.GauntletUI.UIContext.UpdateInput(InputType handleInputs)
at TaleWorlds.Engine.Screens.ScreenManager.Update()
at TaleWorlds.Engine.Screens.ScreenManager.Tick_Patch2(Single dt)";
            private const string StacktraceExample4 =
                @"Exception information
Type: System.TypeInitializationException
Message: The type initializer for 'Bannerlord.BUTR.Shared.Helpers.TextObjectUtils' threw an exception.
Source: Bannerlord.Harmony
CallStack:
at Bannerlord.BUTR.Shared.Helpers.TextObjectUtils.Create(String value)
at Bannerlord.Harmony.SubModule.CheckLoadOrder() in /home/runner/work/Bannerlord.Harmony/Bannerlord.Harmony/src/Bannerlord.Harmony/SubModule.cs:line 87
at Bannerlord.Harmony.SubModule.OnBeforeInitialModuleScreenSetAsRootPostfix(MBSubModuleBase __instance) in /home/runner/work/Bannerlord.Harmony/Bannerlord.Harmony/src/Bannerlord.Harmony/SubModule.cs:line 72
at TaleWorlds.MountAndBlade.Module.SetInitialModuleScreenAsRootScreen()
at TaleWorlds.MountAndBlade.Module.OnApplicationTick_Patch2(Module this, Single dt)


Inner Exception information
Type: System.ArgumentException
Message: Incorrect number of parameters supplied for lambda declaration
Source: System.Core
CallStack:
at System.Linq.Expressions.Expression.ValidateLambdaArgs(Type delegateType, Expression& body, ReadOnlyCollection`1 parameters)
at System.Linq.Expressions.Expression.Lambda[TDelegate](Expression body, String name, Boolean tailCall, IEnumerable`1 parameters)
at System.Linq.Expressions.Expression.Lambda[TDelegate](Expression body, IEnumerable`1 parameters)
at Bannerlord.BUTR.Shared.Helpers.ReflectionHelper.GetDelegate[TDelegate](ConstructorInfo constructorInfo) in /home/runner/work/Bannerlord.Harmony/Bannerlord.Harmony/src/Bannerlord.Harmony/obj/Release/net472/NuGet/F082DFB28C5093D4AE48517DE8ED8B7B8E5B77DD/Bannerlord.BUTR.Shared/1.5.1.5/Bannerlord.BUTR.Shared/Helpers/ReflectionHelper.cs:line 62
at Bannerlord.BUTR.Shared.Helpers.TextObjectUtils..cctor() in /home/runner/work/Bannerlord.Harmony/Bannerlord.Harmony/src/Bannerlord.Harmony/obj/Release/net472/NuGet/F082DFB28C5093D4AE48517DE8ED8B7B8E5B77DD/Bannerlord.BUTR.Shared/1.5.1.5/Bannerlord.BUTR.Shared/Helpers/TextObjectUtils.cs:line 82";

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