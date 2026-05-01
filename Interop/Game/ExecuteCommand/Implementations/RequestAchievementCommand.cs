using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RequestAchievementCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.RequestAchievement;

    /// <summary>
    ///     请求成就进度数据
    /// </summary>
    public void Request(uint achievementRowID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, achievementRowID);
}
