using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class FurnishStateCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.FurnishState;

    /// <summary>
    ///     进入布置家具或庭具状态
    /// </summary>
    public void Enter(uint plotIndex = 0) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 0, plotIndex);
}
