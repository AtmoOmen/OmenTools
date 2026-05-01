using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RepairAllItemsNPCCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.RepairAllItemsNPC;

    /// <summary>
    ///     在 NPC 处批量维修装备
    /// </summary>
    public void Repair(RepairCategory category) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, (uint)category);

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
