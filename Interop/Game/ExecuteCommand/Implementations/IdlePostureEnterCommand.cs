using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class IdlePostureEnterCommand : ExecuteCommandBase
{
    /// <summary>
    ///     进入闲置状态姿势
    /// </summary>
    public static void Enter(uint postureIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.IdlePostureEnter, 0, postureIndex);
}
