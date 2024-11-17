using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Action = Lumina.Excel.Sheets.Action;

namespace OmenTools.Helpers;

public static unsafe partial class HelpersOm
{
    public static bool IsActionUnlocked(uint actionID)
    {
        if (DService.ClientState.LocalPlayer is not { } player) return false;
        var data = LuminaCache.GetRow<Action>(actionID);
        if (data.RowId == 0) return false;

        var unlockLink = data.UnlockLink.RowId;
        var unlockLinkCondition =
            unlockLink == 0 || UIState.Instance()->IsUnlockLinkUnlockedOrQuestCompleted(unlockLink);

        var unlockLevel = data.ClassJobLevel;
        var unlockLevelCondition = unlockLevel == 0 || unlockLevel <= player.Level;

        return unlockLinkCondition && unlockLevelCondition;
    }
}