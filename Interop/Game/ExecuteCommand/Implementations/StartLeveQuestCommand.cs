using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class StartLeveQuestCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.StartLeveQuest;

    /// <summary>
    ///     开始理符任务
    /// </summary>
    public void Start(uint levequestID, uint levelIncrease) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, levequestID, levelIncrease);
}