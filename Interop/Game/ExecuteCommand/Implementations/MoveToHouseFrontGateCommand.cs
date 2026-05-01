using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MoveToHouseFrontGateCommand : ExecuteCommandBase
{
    /// <summary>
    ///     移动到庭院门前
    /// </summary>
    public static void Move(uint plotIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MoveToHouseFrontGate, plotIndex);
}
