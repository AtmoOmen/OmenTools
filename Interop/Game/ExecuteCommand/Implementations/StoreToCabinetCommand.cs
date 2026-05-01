using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class StoreToCabinetCommand : ExecuteCommandBase
{
    /// <summary>
    ///     存入物品至收藏柜
    /// </summary>
    public static void Store(uint cabinetRowID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.StoreToCabinet, cabinetRowID);
}
