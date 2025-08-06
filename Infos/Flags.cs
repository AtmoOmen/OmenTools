﻿using Dalamud.Game.ClientState.Conditions;

namespace OmenTools.Infos;

public static partial class InfosOm
{
    public static bool OccupiedInEvent =>
        DService.Condition.Any(
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
            ConditionFlag.UsingHousingFunctions) || 
        DService.ObjectTable.LocalPlayer?.IsTargetable != true;

    public static bool BetweenAreas => 
        DService.Condition.Any(ConditionFlag.BetweenAreas, ConditionFlag.BetweenAreas51);

    public static bool BoundByDuty =>
        DService.Condition.Any(
            ConditionFlag.BoundByDuty,
            ConditionFlag.BoundByDuty56,
            ConditionFlag.BoundByDuty95,
            ConditionFlag.InDutyQueue);

    public static bool WatchingCutscene =>
        DService.Condition.Any(ConditionFlag.WatchingCutscene, ConditionFlag.WatchingCutscene78);

    public static bool IsCasting => 
        DService.Condition.Any(ConditionFlag.Casting, ConditionFlag.Casting87);

    public static bool IsOnMount => 
        DService.Condition.Any(ConditionFlag.Mounted, ConditionFlag.RidingPillion);

    public static bool CanMount =>
        !DService.Condition.Any(ConditionFlag.Mounted, ConditionFlag.Mounting, ConditionFlag.InCombat, ConditionFlag.Casting);

    public static bool IsOnWorldTravel => 
        DService.Condition.Any(ConditionFlag.WaitingToVisitOtherWorld, ConditionFlag.ReadyingVisitOtherWorld);
}
