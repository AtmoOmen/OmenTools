using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RepairAllItemsNPCCommand : ExecuteCommandBase
{
    /// <summary>
    ///     在 NPC 处批量维修装备
    /// </summary>
    public static void Repair(RepairCategory category) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RepairAllItemsNPC, (uint)category);

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
