using FFXIVClientStructs.FFXIV.Client.Game;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class EventFrameworkActionCommand : ExecuteCommandBase
{
    /// <summary>
    ///     执行分解
    /// </summary>
    public static void Desynthesize() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.EventFrameworkAction, 3735552);

    /// <summary>
    ///     执行回收魔晶石
    /// </summary>
    public static void RetrieveMateria() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.EventFrameworkAction, 3735553);

    /// <summary>
    ///     执行精选
    /// </summary>
    public static void AetherialReduction() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.EventFrameworkAction, 3735554);

    /// <summary>
    ///     修理装备中物品
    /// </summary>
    public static void RepairEquippedItems() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.EventFrameworkAction, 3735555, 2, 1000);

    /// <summary>
    ///     修理分页物品
    /// </summary>
    public static void RepairPage(uint pageIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.EventFrameworkAction, 3735555, 3, pageIndex);

    /// <summary>
    ///     修理单独物品
    /// </summary>
    public static void RepairItem(InventoryType inventoryType, uint inventorySlot, uint itemID, bool isHQ = false) =>
        ExecuteCommandManager.Instance().ExecuteCommand
        (
            ExecuteCommandFlag.EventFrameworkAction,
            3735555,
            inventorySlot << 16 | 1,
            (uint)inventoryType,
            itemID + (isHQ ? 1000000U : 0U)
        );
}
