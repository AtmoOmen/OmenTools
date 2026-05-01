using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class EnterGlamourPlateStateCommand : ExecuteCommandBase
{
    /// <summary>
    ///     进入投影模板选择状态
    /// </summary>
    public static void Enter(bool unknown = true) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.EnterGlamourPlateState, 1, unknown ? 1U : 0U);

    /// <summary>
    ///     退出投影模板选择状态
    /// </summary>
    public static void Exit(bool unknown = true) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.EnterGlamourPlateState, 0, unknown ? 1U : 0U);
}
