using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJIWorkshopAssignCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.MJIWorkshopAssign;

    /// <summary>
    ///     添加无人岛工房排班
    /// </summary>
    public void Add(uint startingHour, uint craftobjectID, uint cycleDay) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, BuildScheduleParam(startingHour, craftobjectID), cycleDay);

    /// <summary>
    ///     删除无人岛工房排班
    /// </summary>
    public void Remove(uint startingHour, uint craftobjectID, uint cycleDay) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, BuildScheduleParam(startingHour, craftobjectID), cycleDay, 0, 1);

    private static uint BuildScheduleParam(uint startingHour, uint craftobjectID) =>
        8 * (startingHour | 32 * craftobjectID);
}
