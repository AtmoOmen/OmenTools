using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class AchievementCommand : ExecuteCommandBase
{
    /// <summary>
    ///     请求成就进度数据
    /// </summary>
    public static void Request(uint achievementRowID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestAchievement, achievementRowID);

    /// <summary>
    ///     请求接近达成成就概览
    /// </summary>
    public static void RequestNearCompletion() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestNearCompletionAchievement, 1);
}
