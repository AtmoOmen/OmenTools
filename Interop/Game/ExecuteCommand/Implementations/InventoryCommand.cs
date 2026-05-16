using FFXIVClientStructs.FFXIV.Client.Game;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class InventoryCommand : ExecuteCommandBase
{
    /// <summary>
    ///     请求指定物品栏数据
    /// </summary>
    public static void Request(InventoryType inventoryType) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestInventory, (uint)inventoryType);

    /// <summary>
    ///     在不同的物品栏间移动物品
    /// </summary>
    public static void Move(InventoryType sourceInventoryType, InventoryType targetInventoryType) =>
        ExecuteCommandManager.Instance().ExecuteCommand
        (
            ExecuteCommandFlag.MoveItemInventory,
            (uint)sourceInventoryType,
            (uint)targetInventoryType
        );

    /// <summary>
    ///     通知物品移动操作受阻
    /// </summary>
    public static void BlockOperation(InventoryType sourceInventoryType, InventoryType targetInventoryType) =>
        ExecuteCommandManager.Instance().ExecuteCommand
        (
            ExecuteCommandFlag.InventoryOperationBlocked,
            (uint)sourceInventoryType,
            (uint)targetInventoryType
        );

    /// <summary>
    ///     完成特定物品栏操作
    /// </summary>
    public static void FinishOperation(InventoryType inventoryType, uint inventorySlot) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.FinishInventoryOperation, (uint)inventoryType, inventorySlot);

    /// <summary>
    ///     恢复被锁定或拦截的物品
    /// </summary>
    public static void RecoverBlockedItem(InventoryType inventoryType, uint inventorySlot) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RecoverBlockedItem, (uint)inventoryType, inventorySlot);

    /// <summary>
    ///     请求陆行鸟鞍囊数据
    /// </summary>
    public static void RequestSaddleBag() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestSaddleBag);

    /// <summary>
    ///     刷新物品栏数据
    /// </summary>
    public static void Refresh() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.InventoryRefresh);
}
