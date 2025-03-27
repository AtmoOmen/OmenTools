using Dalamud.Game.ClientState.Conditions;

namespace OmenTools.Infos;

public static partial class InfosOm
{
    public static bool OccupiedInEvent =>
        DService.Condition[ConditionFlag.Occupied]
        || DService.Condition[ConditionFlag.Occupied30]
        || DService.Condition[ConditionFlag.Occupied33]
        || DService.Condition[ConditionFlag.Occupied38]
        || DService.Condition[ConditionFlag.Occupied39]
        || DService.Condition[ConditionFlag.OccupiedInCutSceneEvent]
        || DService.Condition[ConditionFlag.OccupiedInEvent]
        || DService.Condition[ConditionFlag.OccupiedInQuestEvent]
        || DService.Condition[ConditionFlag.OccupiedSummoningBell]
        || DService.Condition[ConditionFlag.WatchingCutscene]
        || DService.Condition[ConditionFlag.WatchingCutscene78]
        || DService.Condition[ConditionFlag.BetweenAreas]
        || DService.Condition[ConditionFlag.BetweenAreas51]
        || DService.Condition[ConditionFlag.InThatPosition]
        || DService.Condition[ConditionFlag.TradeOpen]
        || DService.Condition[ConditionFlag.Crafting]
        || DService.Condition[ConditionFlag.InThatPosition]
        || DService.Condition[ConditionFlag.Unconscious]
        || DService.Condition[ConditionFlag.MeldingMateria]
        || DService.Condition[ConditionFlag.Gathering]
        || DService.Condition[ConditionFlag.OperatingSiegeMachine]
        || DService.Condition[ConditionFlag.CarryingItem]
        || DService.Condition[ConditionFlag.CarryingObject]
        || DService.Condition[ConditionFlag.BeingMoved]
        || DService.Condition[ConditionFlag.Emoting]
        || DService.Condition[ConditionFlag.Mounted2]
        || DService.Condition[ConditionFlag.Mounting]
        || DService.Condition[ConditionFlag.Mounting71]
        || DService.Condition[ConditionFlag.ParticipatingInCustomMatch]
        || DService.Condition[ConditionFlag.PlayingLordOfVerminion]
        || DService.Condition[ConditionFlag.ChocoboRacing]
        || DService.Condition[ConditionFlag.PlayingMiniGame]
        || DService.Condition[ConditionFlag.Performing]
        || DService.Condition[ConditionFlag.PreparingToCraft]
        || DService.Condition[ConditionFlag.Fishing]
        || DService.Condition[ConditionFlag.Transformed]
        || DService.Condition[ConditionFlag.UsingHousingFunctions]
        || DService.ObjectTable.LocalPlayer?.IsTargetable != true;

    public static bool BetweenAreas => DService.Condition[ConditionFlag.BetweenAreas] || DService.Condition[ConditionFlag.BetweenAreas51];

    public static bool BoundByDuty => DService.Condition[ConditionFlag.BoundByDuty] ||
                                        DService.Condition[ConditionFlag.BoundByDuty56] ||
                                        DService.Condition[ConditionFlag.BoundByDuty95] ||
                                        DService.Condition[ConditionFlag.InDutyQueue];

    public static bool WatchingCutscene => DService.Condition[ConditionFlag.WatchingCutscene] ||
                                           DService.Condition[ConditionFlag.WatchingCutscene78];

    public static bool IsCasting => DService.Condition[ConditionFlag.Casting] || DService.Condition[ConditionFlag.Casting87];

    public static bool IsOnMount => DService.Condition[ConditionFlag.Mounted] || DService.Condition[ConditionFlag.Mounted2];

    public static bool CanMount => !DService.Condition[ConditionFlag.Mounted] && !DService.Condition[ConditionFlag.Mounting] && !DService.Condition[ConditionFlag.InCombat] && !DService.Condition[ConditionFlag.Casting];

    public static bool IsOnWorldTravel => DService.Condition[ConditionFlag.WaitingToVisitOtherWorld] ||
                                          DService.Condition[ConditionFlag.ReadyingVisitOtherWorld];
}
