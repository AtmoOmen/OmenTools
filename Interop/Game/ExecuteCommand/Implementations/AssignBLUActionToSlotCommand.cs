using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class AssignBLUActionToSlotCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.AssignBLUActionToSlot;

    /// <summary>
    ///     应用青魔法师有效技能
    /// </summary>
    public void Apply(uint slotIndex, uint actionID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 0, slotIndex, actionID);

    /// <summary>
    ///     交换青魔法师有效技能
    /// </summary>
    public void Swap(uint slotIndex, uint targetSlotIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 1, slotIndex, targetSlotIndex);
}
