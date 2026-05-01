using FFXIVClientStructs.FFXIV.Client.Game;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class NPCRepairNPCCommand : ExecuteCommandBase
{
    /// <summary>
    ///     在 NPC 处批量维修装备
    /// </summary>
    public static void Repair(RepairCategory category) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RepairAllItemsNPC, (uint)category);
    
    /// <summary>
    ///     在 NPC 处批量维修装备中装备
    /// </summary>
    public static void RepairEquipped() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RepairEquippedItemsNPC, 1000);
    
    /// <summary>
    ///     在 NPC 处维修装备
    /// </summary>
    public static void Repair(InventoryType inventoryType, uint inventorySlot, uint itemID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RepairItemNPC, (uint)inventoryType, inventorySlot, itemID);

    public enum RepairCategory : uint
    {
        MainHandAndOffHand = 0,
        HeadBodyHands      = 1,
        LegsAndFeet        = 2,
        EarAndNeck         = 3,
        WristAndRings      = 4,
        Inventory          = 5
    }
}
