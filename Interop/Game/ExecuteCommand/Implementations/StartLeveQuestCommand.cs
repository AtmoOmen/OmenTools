using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class StartLeveQuestCommand : ExecuteCommandBase
{
    /// <summary>
    ///     开始理符任务
    /// </summary>
    public static void Start(uint levequestID, uint levelIncrease) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.StartLeveQuest, levequestID, levelIncrease);
}
