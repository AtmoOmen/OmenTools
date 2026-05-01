using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJISetRestCyclesCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.MJISetRestCycles;

    /// <summary>
    ///     设置无人岛休息周期
    /// </summary>
    public void Set(uint restDay1, uint restDay2, uint restDay3, uint restDay4) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, restDay1, restDay2, restDay3, restDay4);
}