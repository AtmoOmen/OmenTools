using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class AbandonLeveQuestCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.AbandonLeveQuest;

    /// <summary>
    ///     放弃理符任务
    /// </summary>
    public void Abandon(uint levequestID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, levequestID);
}