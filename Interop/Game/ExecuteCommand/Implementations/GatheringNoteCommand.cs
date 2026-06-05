using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class GatheringNoteCommand : ExecuteCommandBase
{
    /// <summary>
    ///     请求鱼类图鉴数据
    /// </summary>
    public static void RequestFishingNoteInfo(uint fishingNoteInfoID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestFishingNote, fishingNoteInfoID);

    /// <summary>
    ///     请求刺鱼图鉴数据
    /// </summary>
    public static void RequestSpearfishNoteInfo(uint fishingNoteInfoID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestSpearfishNote, fishingNoteInfoID);

    /// <summary>
    ///     请求采集点数据
    /// </summary>
    public static void RequestGatheringPoint(uint gatheringPointID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestGatheringPoint, gatheringPointID);

    /// <summary>
    ///     将采集笔记指定分区指定等级区间标记为已发现过
    /// </summary>
    public static void MarkSeenGatherDivisionLevelRange(uint divisionIndex, uint levelRangeIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MarkGatherDivisionLevelRangeSeen, divisionIndex, levelRangeIndex);
}
