using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using Action = Lumina.Excel.Sheets.Action;
using Control = FFXIVClientStructs.FFXIV.Client.Game.Control.Control;

namespace OmenTools.Helpers;

public static unsafe partial class HelpersOm
{
    /// <summary>
    /// 技能类型均为 <see cref="FFXIVClientStructs.FFXIV.Client.Game.ActionType.Action"/>
    /// </summary>
    /// <returns></returns>
    public static uint GetAdjustSprintActionID()
    {
        if (!LuminaGetter.TryGetRow<GeneralAction>(4, out var row)) return 3;
        
        var localPlayer = Control.GetLocalPlayer();
        if (localPlayer == null || !LuminaGetter.TryGetRow<ClassJob>(localPlayer->ClassJob, out var jobRow)) 
            return row.Action.RowId;
        
        // 开拓无人岛
        if (GameMain.Instance()->CurrentTerritoryIntendedUseId == 49)
            return 31314;
        
        // 宇宙探索
        if (GameMain.Instance()->CurrentTerritoryIntendedUseId == 60 && jobRow.DohDolJobIndex > -1)
            return 43357;
        
        return row.Action.RowId;
    }
    
    public static bool IsActionUnlocked(uint actionID)
    {
        if (DService.ObjectTable.LocalPlayer is not { } player) return false;
        if (!LuminaGetter.TryGetRow<Action>(actionID, out var data)) return false;

        var unlockLink          = data.UnlockLink.RowId;
        var unlockLinkCondition = unlockLink == 0 || UIState.Instance()->IsUnlockLinkUnlockedOrQuestCompleted(unlockLink);

        var unlockLevel          = data.ClassJobLevel;
        var unlockLevelCondition = unlockLevel == 0 || unlockLevel <= player.Level;

        return unlockLinkCondition && unlockLevelCondition;
    }
}
