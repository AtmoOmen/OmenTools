using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RequestPlacardDataCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.RequestPlacardData;

    /// <summary>
    ///     请求门牌数据
    /// </summary>
    public void Request(uint territoryType, uint wardID, uint houseID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, territoryType, wardID * 256 + houseID);
}