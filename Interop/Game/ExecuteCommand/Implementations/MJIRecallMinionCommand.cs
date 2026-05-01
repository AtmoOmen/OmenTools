using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJIRecallMinionCommand : ExecuteCommandBase
{
    /// <summary>
    ///     召回无人岛放生的宠物
    /// </summary>
    public static void Recall(uint minionIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIRecallMinion, minionIndex);
}
