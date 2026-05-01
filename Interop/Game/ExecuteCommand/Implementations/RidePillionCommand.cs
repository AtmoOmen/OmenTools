using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RidePillionCommand : ExecuteCommandBase
{
    /// <summary>
    ///     共同骑乘指定目标的位置
    /// </summary>
    public static void Ride(uint targetID, uint seatIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RidePillion, targetID, seatIndex);
}
