using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJIRecallMinionCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.MJIRecallMinion;

    /// <summary>
    ///     召回无人岛放生的宠物
    /// </summary>
    public void Recall(uint minionIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, minionIndex);
}
