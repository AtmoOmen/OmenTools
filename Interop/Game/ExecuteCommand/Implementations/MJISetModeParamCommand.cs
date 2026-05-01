using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJISetModeParamCommand : ExecuteCommandBase
{
    /// <summary>
    ///     设置无人岛模式参数
    /// </summary>
    public static void Set(uint param) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJISetModeParam, param);

    /// <summary>
    ///     清除无人岛模式参数
    /// </summary>
    public static void Clear() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJISetModeParam);
}
