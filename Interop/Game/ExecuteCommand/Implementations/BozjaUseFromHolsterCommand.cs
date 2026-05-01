using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class BozjaUseFromHolsterCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.BozjaUseFromHolster;

    /// <summary>
    ///     博兹雅分配失传技能库到技能槽
    /// </summary>
    public void Use(uint holsterIndex, uint slotIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, holsterIndex, slotIndex);
}