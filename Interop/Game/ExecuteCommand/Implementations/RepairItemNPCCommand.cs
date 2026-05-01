using FFXIVClientStructs.FFXIV.Client.Game;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RepairItemNPCCommand : ExecuteCommandBase
{
    /// <summary>
    ///     在 NPC 处维修装备
    /// </summary>
    public static void Repair(InventoryType inventoryType, uint inventorySlot, uint itemID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RepairItemNPC, (uint)inventoryType, inventorySlot, itemID);
}
