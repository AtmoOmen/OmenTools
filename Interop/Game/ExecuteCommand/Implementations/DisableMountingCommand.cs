using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class DisableMountingCommand : ExecuteCommandBase
{
    /// <summary>
    ///     赋予禁止骑乘坐骑状态
    /// </summary>
    public static void Apply() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.DisableMounting, 1);

    /// <summary>
    ///     移除禁止骑乘坐骑状态
    /// </summary>
    public static void Remove() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.DisableMounting);

    /// <summary>
    ///     赋予或取消禁止骑乘坐骑状态
    /// </summary>
    public static void Toggle(bool isDisabled) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.DisableMounting, isDisabled ? 1U : 0U);
}
