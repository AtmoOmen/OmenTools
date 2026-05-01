using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class StartSoloQuestBattleCommand : ExecuteCommandBase
{
    /// <summary>
    ///     发送单人任务战斗请求
    /// </summary>
    public static void Start(Difficulty difficulty = Difficulty.Normal) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.StartSoloQuestBattle, (uint)difficulty);

    public enum Difficulty : uint
    {
        Normal   = 0,
        Easy     = 1,
        VeryEasy = 2
    }
}
