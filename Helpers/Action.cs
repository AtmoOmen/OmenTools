using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Action = Lumina.Excel.Sheets.Action;

namespace OmenTools.Helpers;

public static unsafe partial class HelpersOm
{
    public static bool IsActionUnlocked(uint actionID)
    {
        if (DService.ClientState.LocalPlayer is not { } player) return false;
        var data = LuminaCache.GetRow<Action>(actionID);
        if (data == null) return false;

        var unlockLink = data?.UnlockLink.RowId ?? 0;
        var unlockLinkCondition =
            unlockLink == 0 || UIState.Instance()->IsUnlockLinkUnlockedOrQuestCompleted(unlockLink);

        var unlockLevel = data?.ClassJobLevel ?? 0;
        var unlockLevelCondition = unlockLevel == 0 || unlockLevel <= player.Level;

        return unlockLinkCondition && unlockLevelCondition;
    }
}
