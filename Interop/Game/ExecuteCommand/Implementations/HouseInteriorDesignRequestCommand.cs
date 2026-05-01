using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class HouseInteriorDesignRequestCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.HouseInteriorDesignRequest;

    /// <summary>
    ///     请求房屋内部改建信息
    /// </summary>
    public void Request(uint houseIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, houseIndex);
}
