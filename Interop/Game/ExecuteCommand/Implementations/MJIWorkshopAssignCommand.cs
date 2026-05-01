using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJIWorkshopAssignCommand : ExecuteCommandBase
{
    /// <summary>
    ///     添加无人岛工房排班
    /// </summary>
    public static void Add(uint startingHour, uint craftobjectID, uint cycleDay) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIWorkshopAssign, BuildScheduleParam(startingHour, craftobjectID), cycleDay);

    /// <summary>
    ///     删除无人岛工房排班
    /// </summary>
    public static void Remove(uint startingHour, uint craftobjectID, uint cycleDay) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIWorkshopAssign, BuildScheduleParam(startingHour, craftobjectID), cycleDay, 0, 1);

    private static uint BuildScheduleParam(uint startingHour, uint craftobjectID) =>
        8 * (startingHour | 32 * craftobjectID);
}
