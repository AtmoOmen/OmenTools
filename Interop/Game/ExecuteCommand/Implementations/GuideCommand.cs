using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class GuideCommand : ExecuteCommandBase
{
    /// <summary>
    ///     向服务器标记已展示过某一新手指南
    /// </summary>
    public static void MarkHowToFinished(uint howToID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MarkHowToSeen, howToID);
}
