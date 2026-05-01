using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class IdlePostureChangeCommand : ExecuteCommandBase
{
    /// <summary>
    ///     更改闲置状态姿势
    /// </summary>
    public static void Change(uint postureIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.IdlePostureChange, 0, postureIndex);
}
