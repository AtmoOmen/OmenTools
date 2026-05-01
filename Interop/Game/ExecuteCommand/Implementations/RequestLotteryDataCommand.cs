using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RequestLotteryDataCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.RequestLotteryData;

    /// <summary>
    ///     请求抽选数据
    /// </summary>
    public void Request(uint territoryType, uint wardID, uint houseID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, territoryType, wardID * 256 + houseID);
}
