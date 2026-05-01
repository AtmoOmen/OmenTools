using FFXIVClientStructs.FFXIV.Client.Game;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RequestInventoryCommand : ExecuteCommandBase
{
    /// <summary>
    ///     请求指定物品栏数据
    /// </summary>
    public static void Request(InventoryType inventoryType) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestInventory, (uint)inventoryType);
}
