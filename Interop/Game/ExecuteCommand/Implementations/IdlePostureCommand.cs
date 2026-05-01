using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class IdlePostureCommand : ExecuteCommandBase
{
    /// <summary>
    ///     更改闲置状态姿势
    /// </summary>
    public static void Change(uint postureIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.IdlePostureChange, 0, postureIndex);
    
    /// <summary>
    ///     进入闲置状态姿势
    /// </summary>
    public static void Enter(uint postureIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.IdlePostureEnter, 0, postureIndex);

    /// <summary>
    /// 退出闲置状态姿势
    /// </summary>
    public static void Exit() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.IdlePostureExit);
}
