using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class BozjaUseFromHolsterCommand : ExecuteCommandBase
{
    /// <summary>
    ///     分配博兹雅失传技能库到技能槽
    /// </summary>
    public static void Use(uint holsterIndex, uint slotIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.BozjaUseFromHolster, holsterIndex, slotIndex);
}
