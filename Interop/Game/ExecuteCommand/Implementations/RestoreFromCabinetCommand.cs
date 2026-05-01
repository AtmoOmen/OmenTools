using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RestoreFromCabinetCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.RestoreFromCabinet;

    /// <summary>
    ///     从收藏柜中取回物品
    /// </summary>
    public void Restore(uint cabinetRowID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, cabinetRowID);
}