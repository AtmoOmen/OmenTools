using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RestoreFromCabinetCommand : ExecuteCommandBase
{
    /// <summary>
    ///     从收藏柜中取回物品
    /// </summary>
    public static void Restore(uint cabinetRowID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RestoreFromCabinet, cabinetRowID);
}
