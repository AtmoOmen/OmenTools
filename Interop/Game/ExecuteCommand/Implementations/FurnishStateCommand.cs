using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class FurnishStateCommand : ExecuteCommandBase
{
    /// <summary>
    ///     进入布置家具或庭具状态
    /// </summary>
    public static void Enter(uint plotIndex = 0) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.FurnishState, 0, plotIndex);
}
