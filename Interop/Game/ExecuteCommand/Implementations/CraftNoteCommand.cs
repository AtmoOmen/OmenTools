using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class CraftNoteCommand : ExecuteCommandBase
{
    /// <summary>
    ///     将制作笔记指定分区指定等级区间标记为已发现过
    /// </summary>
    public static void MarkSeenDivisionLevelRange(uint divisionIndex, uint levelRangeIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MarkCraftDivisionLevelRangeSeen, divisionIndex, levelRangeIndex);

    /// <summary>
    ///     中止或完成简易制作
    /// </summary>
    public static void LeaveQuickSynthesis() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.LeaveQuickSynthesis);
}
