using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class IdlePostureChangeCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.IdlePostureChange;

    /// <summary>
    ///     更改闲置状态姿势
    /// </summary>
    public void Change(uint postureIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 0, postureIndex);
}