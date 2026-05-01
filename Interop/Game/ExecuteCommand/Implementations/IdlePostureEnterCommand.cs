using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class IdlePostureEnterCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.IdlePostureEnter;

    /// <summary>
    ///     进入闲置状态姿势
    /// </summary>
    public void Enter(uint postureIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 0, postureIndex);
}