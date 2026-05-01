using FFXIVClientStructs.FFXIV.Client.Game;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class EventFrameworkActionCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.EventFrameworkAction;

    /// <summary>
    ///     执行分解
    /// </summary>
    public void Desynthesize() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 3735552);

    /// <summary>
    ///     执行回收魔晶石
    /// </summary>
    public void RetrieveMateria() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 3735553);

    /// <summary>
    ///     执行精选
    /// </summary>
    public void AetherialReduction() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 3735554);

    /// <summary>
    ///     修理装备中物品
    /// </summary>
    public void RepairEquippedItems() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 3735555, 2, 1000);

    /// <summary>
    ///     修理分页物品
    /// </summary>
    public void RepairPage(uint pageIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 3735555, 3, pageIndex);

    /// <summary>
    ///     修理单独物品
    /// </summary>
    public void RepairItem(InventoryType inventoryType, uint inventorySlot, uint itemID, bool isHQ = false) =>
        ExecuteCommandManager.Instance().ExecuteCommand
        (
            Flag,
            3735555,
            inventorySlot << 16 | 1,
            (uint)inventoryType,
            itemID + (isHQ ? 1000000U : 0U)
        );
}
