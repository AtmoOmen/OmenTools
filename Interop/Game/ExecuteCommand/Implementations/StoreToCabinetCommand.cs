using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class StoreToCabinetCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.StoreToCabinet;

    /// <summary>
    ///     存入物品至收藏柜
    /// </summary>
    public void Store(uint cabinetRowID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, cabinetRowID);
}