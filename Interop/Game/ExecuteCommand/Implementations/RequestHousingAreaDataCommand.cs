using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RequestHousingAreaDataCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.RequestHousingAreaData;

    /// <summary>
    ///     请求住宅区数据
    /// </summary>
    public void Request(uint territoryType, uint wardIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, territoryType, wardIndex);
}