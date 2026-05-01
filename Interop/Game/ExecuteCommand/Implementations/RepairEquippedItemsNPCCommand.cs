using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RepairEquippedItemsNPCCommand : ExecuteCommandBase
{
    /// <summary>
    ///     在 NPC 处批量维修装备中装备
    /// </summary>
    public static void Repair() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RepairEquippedItemsNPC, 1000);
}
