using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJISetModeParamCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.MJISetModeParam;

    /// <summary>
    ///     设置无人岛模式参数
    /// </summary>
    public void Set(uint param) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, param);

    /// <summary>
    ///     清除无人岛模式参数
    /// </summary>
    public void Clear() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag);
}