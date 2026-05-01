using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class DisableMountingCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.DisableMounting;

    /// <summary>
    ///     赋予禁止骑乘坐骑状态
    /// </summary>
    public void Apply() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 1);

    /// <summary>
    ///     移除禁止骑乘坐骑状态
    /// </summary>
    public void Remove() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag);

    /// <summary>
    ///     赋予或取消禁止骑乘坐骑状态
    /// </summary>
    public void Toggle(bool isDisabled) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, isDisabled ? 1U : 0U);
}
