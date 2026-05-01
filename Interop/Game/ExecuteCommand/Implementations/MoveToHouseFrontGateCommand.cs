using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MoveToHouseFrontGateCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.MoveToHouseFrontGate;

    /// <summary>
    ///     移动到庭院门前
    /// </summary>
    public void Move(uint plotIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, plotIndex);
}