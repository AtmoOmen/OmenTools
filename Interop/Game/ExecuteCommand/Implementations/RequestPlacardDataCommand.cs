using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RequestPlacardDataCommand : ExecuteCommandBase
{
    /// <summary>
    ///     请求门牌数据
    /// </summary>
    public static void Request(uint territoryType, uint wardID, uint houseID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestPlacardData, territoryType, wardID * 256 + houseID);
}
