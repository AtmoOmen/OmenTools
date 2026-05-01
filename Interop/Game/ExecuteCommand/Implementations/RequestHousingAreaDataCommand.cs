using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RequestHousingAreaDataCommand : ExecuteCommandBase
{
    /// <summary>
    ///     请求住宅区数据
    /// </summary>
    public static void Request(uint territoryType, uint wardIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestHousingAreaData, territoryType, wardIndex);
}
