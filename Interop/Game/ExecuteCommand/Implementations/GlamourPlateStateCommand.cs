using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class GlamourPlateStateCommand : ExecuteCommandBase
{
    /// <summary>
    ///     进入投影模板选择状态
    /// </summary>
    public static void Enter() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.GlamourPlateState, 1, 1);

    /// <summary>
    ///     退出投影模板选择状态
    /// </summary>
    public static void Exit() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.GlamourPlateState, 0, 1);

    /// <summary>
    /// 进入/退出投影模板选择状态
    /// </summary>
    public static void Toggle(bool isEnter) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.GlamourPlateState, isEnter ? 1U : 0, 1);
}
