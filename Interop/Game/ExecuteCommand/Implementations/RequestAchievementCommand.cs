using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RequestAchievementCommand : ExecuteCommandBase
{
    /// <summary>
    ///     请求成就进度数据
    /// </summary>
    public static void Request(uint achievementRowID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestAchievement, achievementRowID);
}
