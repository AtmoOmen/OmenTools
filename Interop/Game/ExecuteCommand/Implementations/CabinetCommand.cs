using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class CabinetCommand : ExecuteCommandBase
{
    /// <summary>
    ///     从收藏柜中取回物品
    /// </summary>
    public static void Restore(uint cabinetRowID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RestoreFromCabinet, cabinetRowID);

    /// <summary>
    ///     存入物品至收藏柜
    /// </summary>
    public static void Store(uint cabinetRowID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.StoreToCabinet, cabinetRowID);
}
