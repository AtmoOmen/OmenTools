using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class ContentsNoteCommand : ExecuteCommandBase
{
    /// <summary>
    ///     请求挑战笔记数据
    /// </summary>
    public static void Request() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestContentsNote);

    /// <summary>
    ///     请求挑战笔记具体类别下数据
    /// </summary>
    public static void RequestCategory(uint categoryIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.RequestContentsNoteCategory, categoryIndex);
}
