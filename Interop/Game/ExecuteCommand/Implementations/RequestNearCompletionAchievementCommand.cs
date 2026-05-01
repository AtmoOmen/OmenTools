using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class RequestNearCompletionAchievementCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.RequestNearCompletionAchievement;

    /// <summary>
    ///     请求接近达成成就概览
    /// </summary>
    public void Request() =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, 1);
}