using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class WKSChangeModeCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.WKSChangeMode;

    /// <summary>
    ///     变更宇宙探索模式
    /// </summary>
    public void Change(uint modeIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, modeIndex);
}