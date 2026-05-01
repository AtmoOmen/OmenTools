using FFXIVClientStructs.FFXIV.Client.Game;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RepairItemNPCCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.RepairItemNPC;

    /// <summary>
    ///     在 NPC 处维修装备
    /// </summary>
    public void Repair(InventoryType inventoryType, uint inventorySlot, uint itemID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, (uint)inventoryType, inventorySlot, itemID);
}