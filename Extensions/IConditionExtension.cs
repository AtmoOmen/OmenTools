using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;

namespace OmenTools.Extensions;

public static class IConditionExtension
{
    extension(ICondition service)
    {
        public bool IsOccupiedInEvent =>
            service.Any
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

        public bool IsBetweenAreas =>
            service.Any(ConditionFlag.BetweenAreas, ConditionFlag.BetweenAreas51);

        public bool IsBoundByDuty =>
            service.Any
            (
                ConditionFlag.BoundByDuty,
                ConditionFlag.BoundByDuty56,
                ConditionFlag.BoundByDuty95,
                ConditionFlag.InDutyQueue,
                ConditionFlag.WaitingForDuty,
                ConditionFlag.WaitingForDutyFinder
            );

        public bool IsWatchingCutscene =>
            service.Any(ConditionFlag.WatchingCutscene, ConditionFlag.WatchingCutscene78);

        public bool IsCasting =>
            service.Any(ConditionFlag.Casting, ConditionFlag.Casting87);

        public bool IsOnMount =>
            service.Any(ConditionFlag.Mounted, ConditionFlag.RidingPillion);

        public bool IsAbleToMount =>
            !service.Any(ConditionFlag.Mounted, ConditionFlag.Mounting, ConditionFlag.InCombat, ConditionFlag.Casting);

        public bool IsOnWorldTravel =>
            service.Any(ConditionFlag.WaitingToVisitOtherWorld, ConditionFlag.ReadyingVisitOtherWorld);
    }
}
