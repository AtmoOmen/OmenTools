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
    ///     请求已完成成就概览
    /// </summary>
    public static void RequestCompleted() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestCompletedAchievement);

    /// <summary>
    ///     请求全部成就数据
    /// </summary>
    public static void RequestAll() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestAllAchievements);

    /// <summary>
    ///     请求接近达成成就概览
    /// </summary>
    public static void RequestNearCompletion() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestNearCompletedAchievement, 1);

    /// <summary>
    ///     根据页面索引请求 FATE 关联成就数据
    /// </summary>
    public static void RequestFateProgress(uint pageIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestFateProgressAchievement, pageIndex);

    /// <summary>
    ///     请求成就进度数据 (特殊)
    /// </summary>
    public static void RequestSpecial(uint achievementRowID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestAchievementSpecial, achievementRowID);
}
