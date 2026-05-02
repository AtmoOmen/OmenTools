using FFXIVClientStructs.FFXIV.Client.Game;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RepairCommand : ExecuteCommandBase
{
    /// <summary>
    ///     发送修理委托
    /// </summary>
    public static void SendRequest(uint targetEntityID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.SendRepairRequest, targetEntityID);

    /// <summary>
    ///     取消修理委托
    /// </summary>
    public static void CancelRequest(uint targetEntityID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.CancelRepairRequest, targetEntityID);

    /// <summary>
    ///     修理装备中物品
    /// </summary>
    public static void RepairEquippedItems() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.EventFrameworkAction, 3735555, 2, 1000);

    /// <summary>
    ///     修理分页物品
    /// </summary>
    public static void RepairPage(RepairPageCategory category) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.EventFrameworkAction, 3735555, 3, (uint)category);

    /// <summary>
    ///     修理单独物品
    /// </summary>
    public static unsafe void RepairItem(InventoryType inventoryType, uint inventorySlot) =>
        ExecuteCommandManager.Instance().ExecuteCommand
        (
            ExecuteCommandFlag.EventFrameworkAction,
            3735555,
            inventorySlot << 16 | 1,
            (uint)inventoryType,
            InventoryManager.Instance()->GetInventorySlot(inventoryType, (int)inventorySlot)->GetItemId()
        );

    /// <summary>
    ///     在 NPC 处批量维修装备
    /// </summary>
    public static void RepairPageNPC(RepairPageCategory category) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RepairAllItemsNPC, (uint)category);

    /// <summary>
    ///     在 NPC 处批量维修装备中装备
    /// </summary>
    public static void RepairEquippedNPC() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RepairEquippedItemsNPC, 1000);

    /// <summary>
    ///     在 NPC 处维修装备
    /// </summary>
    public static unsafe void RepairNPC(InventoryType inventoryType, uint inventorySlot) =>
        ExecuteCommandManager.Instance().ExecuteCommand
        (
            ExecuteCommandFlag.RepairItemNPC,
            (uint)inventoryType,
            inventorySlot,
            InventoryManager.Instance()->GetInventorySlot(inventoryType, (int)inventorySlot)->GetItemId()
        );

    public enum RepairPageCategory : uint
    {
        /// <summary>
        ///     主手/副手
        /// </summary>
        MainOffHands = 0,

        /// <summary>
        ///     头部/身体/手臂
        /// </summary>
        HeadBodyArms = 1,

        /// <summary>
        ///     腿部/脚部
        /// </summary>
        LegsFeet = 2,

        /// <summary>
        ///     耳部/颈部
        /// </summary>
        EarsNeck = 3,

        /// <summary>
        ///     腕部/戒指
        /// </summary>
        WristsRings = 4,

        /// <summary>
        ///     物品
        /// </summary>
        Inventory = 5
    }
}
