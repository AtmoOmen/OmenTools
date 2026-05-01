using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class WKSChangeModeCommand : ExecuteCommandBase
{
    /// <summary>
    ///     变更宇宙探索模式
    /// </summary>
    public static void Change(uint modeIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.WKSChangeMode, modeIndex);
}
