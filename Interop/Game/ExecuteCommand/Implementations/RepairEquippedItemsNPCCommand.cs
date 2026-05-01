using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RepairEquippedItemsNPCCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.RepairEquippedItemsNPC;

    /// <summary>
    ///     在 NPC 处批量维修装备中装备
    /// </summary>
    public void Repair() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 1000);
}