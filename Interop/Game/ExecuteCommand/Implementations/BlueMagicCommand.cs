using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class BlueMagicCommand : ExecuteCommandBase
{
    /// <summary>
    ///     应用青魔法师有效技能
    /// </summary>
    public static void Apply(uint slotIndex, uint actionID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.AssignBLUActionToSlot, 0, slotIndex, actionID);

    /// <summary>
    ///     交换青魔法师有效技能
    /// </summary>
    public static void Swap(uint slotIndex, uint targetSlotIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.AssignBLUActionToSlot, 1, slotIndex, targetSlotIndex);
}
