using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJIWorkshopCancelCommand : ExecuteCommandBase
{
    /// <summary>
    ///     取消无人岛工坊排班
    /// </summary>
    public static void Cancel(uint startingHour, uint craftobjectID, uint cycleDay) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIWorkshopCancel, BuildScheduleParam(startingHour, craftobjectID), cycleDay);

    private static uint BuildScheduleParam(uint startingHour, uint craftobjectID) =>
        8 * (startingHour | 32 * craftobjectID);
}
