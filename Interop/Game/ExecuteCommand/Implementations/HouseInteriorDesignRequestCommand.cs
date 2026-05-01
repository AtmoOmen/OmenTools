using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class HouseInteriorDesignRequestCommand : ExecuteCommandBase
{
    /// <summary>
    ///     请求房屋内部改建信息
    /// </summary>
    public static void Request(uint houseIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.HouseInteriorDesignRequest, houseIndex);
}
