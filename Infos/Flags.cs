using Dalamud.Game.ClientState.Conditions;

namespace OmenTools.Infos;

public static partial class InfosOm
{
    public static bool OccupiedInEvent =>
        DService.Instance().Condition.Any
        (
            ConditionFlag.Occupied,
            ConditionFlag.Occupied30,
            ConditionFlag.Occupied33,
            ConditionFlag.Occupied38,
            ConditionFlag.Occupied39,
            ConditionFlag.OccupiedInCutSceneEvent,
            ConditionFlag.OccupiedInEvent,
            ConditionFlag.OccupiedInQuestEvent,
            ConditionFlag.OccupiedSummoningBell,
            ConditionFlag.WatchingCutscene,
            ConditionFlag.WatchingCutscene78,
            ConditionFlag.BetweenAreas,
            ConditionFlag.BetweenAreas51,
            ConditionFlag.InThatPosition,
            ConditionFlag.TradeOpen,
            ConditionFlag.Crafting,
            ConditionFlag.Unconscious,
            ConditionFlag.MeldingMateria,
            ConditionFlag.Gathering,
            ConditionFlag.OperatingSiegeMachine,
            ConditionFlag.CarryingItem,
            ConditionFlag.CarryingObject,
            ConditionFlag.BeingMoved,
            ConditionFlag.Emoting,
            ConditionFlag.RidingPillion,
            ConditionFlag.Mounting,
            ConditionFlag.Mounting71,
            ConditionFlag.ParticipatingInCustomMatch,
            ConditionFlag.PlayingLordOfVerminion,
            ConditionFlag.ChocoboRacing,
            ConditionFlag.PlayingMiniGame,
            ConditionFlag.Performing,
            ConditionFlag.PreparingToCraft,
            ConditionFlag.Fishing,
            ConditionFlag.Transformed,
            ConditionFlag.UsingHousingFunctions
        );

    public static bool BetweenAreas =>
        DService.Instance().Condition.Any(ConditionFlag.BetweenAreas, ConditionFlag.BetweenAreas51);

    public static bool BoundByDuty =>
        DService.Instance().Condition.Any
        (
            ConditionFlag.BoundByDuty,
            ConditionFlag.BoundByDuty56,
            ConditionFlag.BoundByDuty95,
            ConditionFlag.InDutyQueue,
            ConditionFlag.WaitingForDuty,
            ConditionFlag.WaitingForDutyFinder
        );

    public static bool WatchingCutscene =>
        DService.Instance().Condition.Any(ConditionFlag.WatchingCutscene, ConditionFlag.WatchingCutscene78);

    public static bool IsCasting =>
        DService.Instance().Condition.Any(ConditionFlag.Casting, ConditionFlag.Casting87);

    public static bool IsOnMount =>
        DService.Instance().Condition.Any(ConditionFlag.Mounted, ConditionFlag.RidingPillion);

    public static bool CanMount =>
        !DService.Instance().Condition.Any(ConditionFlag.Mounted, ConditionFlag.Mounting, ConditionFlag.InCombat, ConditionFlag.Casting);

    public static bool IsOnWorldTravel =>
        DService.Instance().Condition.Any(ConditionFlag.WaitingToVisitOtherWorld, ConditionFlag.ReadyingVisitOtherWorld);
}
