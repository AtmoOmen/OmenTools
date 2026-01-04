using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using LuminaAction = Lumina.Excel.Sheets.Action;
using TerritoryIntendedUse = FFXIVClientStructs.FFXIV.Client.Enums.TerritoryIntendedUse;

namespace OmenTools.Helpers;

public static class ActionManagerExtensions
{
    extension(ref ActionManager manager)
    {
        public static uint GetAdjustSprintActionID()
        {
            if (!LuminaGetter.TryGetRow<GeneralAction>(4, out var row)) return 3;
        
            if (LocalPlayerState.ClassJob == 0) 
                return row.Action.RowId;

            return GameState.TerritoryIntendedUse switch
            {
                // 开拓无人岛
                TerritoryIntendedUse.IslandSanctuary => 31314,
                // 宇宙探索
                TerritoryIntendedUse.CosmicExploration when LocalPlayerState.ClassJobData.DohDolJobIndex > -1 => 43357,
                // 默认
                _ => row.Action.RowId
            };
        }

        public static unsafe bool IsActionUnlocked(uint actionID)
        {
            if (!LuminaGetter.TryGetRow<LuminaAction>(actionID, out var data)) return false;

            var unlockLink          = data.UnlockLink.RowId;
            var unlockLinkCondition = unlockLink == 0 || UIState.Instance()->IsUnlockLinkUnlockedOrQuestCompleted(unlockLink);

            var unlockLevel          = data.ClassJobLevel;
            var unlockLevelCondition = unlockLevel == 0 || unlockLevel <= LocalPlayerState.GetClassJobLevel(data.ClassJob.RowId);

            return unlockLinkCondition && unlockLevelCondition;
        }
    }
}
