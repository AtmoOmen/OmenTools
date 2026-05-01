using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RidePillionCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.RidePillion;

    /// <summary>
    ///     共同骑乘指定目标的位置
    /// </summary>
    public void Ride(uint targetID, uint seatIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, targetID, seatIndex);
}
