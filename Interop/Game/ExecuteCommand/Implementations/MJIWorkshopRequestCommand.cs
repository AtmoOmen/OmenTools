using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJIWorkshopRequestCommand : ExecuteCommandBase
{
    /// <summary>
    ///     请求无人岛工房排班数据
    /// </summary>
    public static void Request(uint cycleDay) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIWorkshopRequest, cycleDay);
}
