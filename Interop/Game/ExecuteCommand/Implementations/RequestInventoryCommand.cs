using FFXIVClientStructs.FFXIV.Client.Game;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RequestInventoryCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.RequestInventory;

    /// <summary>
    ///     请求指定物品栏数据
    /// </summary>
    public void Request(InventoryType inventoryType) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, (uint)inventoryType);
}